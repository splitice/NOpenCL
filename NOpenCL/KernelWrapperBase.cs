#region License and Copyright Notice
// Copyright (c) 2010 Ananth B.
// All rights reserved.
// 
// The contents of this file are made available under the terms of the
// Eclipse Public License v1.0 (the "License") which accompanies this
// distribution, and is available at the following URL:
// http://www.opensource.org/licenses/eclipse-1.0.php
// 
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either expressed or implied. See the License for
// the specific language governing rights and limitations under the License.
// 
// By using this software in any fashion, you are agreeing to be bound by the
// terms of the License.
#endregion

using System;
using System.Runtime.InteropServices;

namespace NOpenCL
{
    public abstract class KernelWrapperBase
    {
        private Kernel _kernel;
        private readonly Context _context;
        
        protected KernelWrapperBase(Context context)
        {
            _context = context;
            Compile(KernelSource, KernelName);
        }

        protected uint GetWorkDimension(uint x, uint y, uint z)
        {
            return (uint)((x > 0 ? 1 : 0) + (y > 0 ? 1 : 0) + (z > 0 ? 1 : 0));
        }

        protected IntPtr[] GetWorkSizes(uint x, uint y, uint z)
        {
            var sum = GetWorkDimension(x, y, z);
            switch (sum)
            {
                case 0:
                    return null;

                case 1:
                    return new[] { (IntPtr)x };

                case 2:
                    return new[] { (IntPtr)x, (IntPtr)y };

                case 3:
                    return new[] { (IntPtr)x, (IntPtr)y, (IntPtr)z };

                default:
                    ErrorHandler.ThrowOnFailure(UnsafeNativeMethods.ErrorCode.InvalidWorkDimension);
                    return null;
            }
        }

        internal void Compile(string source, string kernelName)
        {
            var program = _context.CreateProgramWithSource(source);
        }


        internal void CompileDebug(string source, string kernelName, Device device)
        {
            var program = _context.CreateProgramWithSource(source);
            try
            {
                program.Build(new Device[] { device });
                if (program.GetBuildStatus(device) == BuildStatus.Error)
                {
                    throw new Exception("Error building: " + program.GetBuildLog(device));
                }
                _kernel = program.CreateKernel(kernelName);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception occured when building kernel. Error: " + program.GetBuildLog(device),ex);
            }
        }

        protected internal abstract string KernelPath { get; }
        protected internal abstract string OriginalKernelPath { get; }
        protected internal abstract string KernelSource { get; }
        protected internal abstract string KernelName { get; }

        public Context Context { get { return _context; } }
        public Kernel Kernel { get { return _kernel; } }
    }
}
