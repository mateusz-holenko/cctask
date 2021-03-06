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
using CCTask.Compilers;
using CCTask.Linkers;

namespace CCTask
{
	internal sealed class CompilerProvider
	{
		internal static CompilerProvider Instance { get; private set; }

		internal ICompiler CCompiler { get; private set; }
		internal ILinker CLinker { get; private set; }

		static CompilerProvider()
		{
			Instance = new CompilerProvider();
		}

		private CompilerProvider()
		{
			cCompilerPath = "cc"; // TODO: check existence
			cLinkerPath = "cc"; // yup, that's what I meant here
			GetCompilingExecs();
		}

		private void GetCompilingExecs()
		{
			CCompiler = new GCC(cCompilerPath);
			CLinker = new GLD(cLinkerPath);
		}

		private readonly string cCompilerPath;
		private readonly string cLinkerPath;
	}
}

