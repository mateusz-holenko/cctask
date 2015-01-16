/**
 * CCTask
 * 
 * Copyright 2012 Konrad Kruczyński <konrad.kruczynski@gmail.com>
 * 
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:

 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */ 
using System;
using System.Linq;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CCTask.Compilers;

namespace CCTask
{
	public class CCompilerTask : Task
	{
		[Required]
		public ITaskItem[] Sources { get; set; }

		[Output]
		public ITaskItem[] ObjectFiles { get; set; }

		public string ObjectFilesDirectory { get; set; }

		public ITaskItem[] CompilationFlags   { get; set; }
		public ITaskItem[] ConfigurationFlags { get; set; }

		public bool Verbose { get; set; }

		public bool Parallel { get; set; }

		public CCompilerTask()
		{
			Parallel = true;

			compiler = CompilerProvider.Instance.CCompiler;
			regex = new Regex(@"\.c$");

			configurationFlags = (ConfigurationFlags != null && ConfigurationFlags.Any()) ? ConfigurationFlags.Aggregate(string.Empty, (curr, next) => string.Format("{0} {1}", curr, next.ItemSpec)) : string.Empty;
			compilationFlags = (CompilationFlags != null && CompilationFlags.Any()) ? CompilationFlags.Aggregate(string.Empty, (curr, next) => string.Format("{0} {1}", curr, next.ItemSpec)) : string.Empty;
		}

		public override bool Execute()
		{
			Logger.Instance = new XBuildLogProvider(Log, Verbose); // TODO: maybe initialise statically

			using (var cache = new FileCacheManager(ObjectFilesDirectory))
			{
				var objectFiles = new List<string>();
				if (Parallel)
				{
					var compilationResult = System.Threading.Tasks.Parallel.ForEach(Sources.Select(x => x.ItemSpec), (source, loopState) => {
						if(!CompileSingleFile(source, objectFiles, cache))
						{
							loopState.Break();
						}
					});
					if (compilationResult.LowestBreakIteration != null)
					{
						return false;
					}
				}
				else
				{
					foreach (var source in Sources.Select(x => x.ItemSpec))
					{
						if (!CompileSingleFile(source, objectFiles, cache))
						{
							return false;
						}
					}
				}

				ObjectFiles = objectFiles.Any() ? objectFiles.Select(x => new TaskItem(x)).ToArray() : new TaskItem[0];

				return true;
			}
		}

		private bool CompileSingleFile(string source, List<string> objectFiles, FileCacheManager cache)
		{
			var objectFile = ObjectFilesDirectory == null ? regex.Replace(source, ".o") : string.Format("{0}/{1}", ObjectFilesDirectory, regex.Replace(source, ".o"));
			bool skipped;
			if (!compiler.Compile(source, objectFile, configurationFlags, compilationFlags, cache.SourceHasChanged, out skipped))
			{
				return false;
			}

			if (!skipped)
			{
				lock (objectFiles)
				{
					objectFiles.Add(objectFile);
				}
			}

			return true;
		}

		private readonly string configurationFlags;
		private readonly string compilationFlags;
		private readonly Regex regex;
		private readonly ICompiler compiler;
	}
}

