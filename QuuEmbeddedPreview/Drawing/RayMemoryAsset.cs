using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuuEmbeddedPreview.Drawing
{
    class RayMemoryAsset : IDisposable
    {
        public RayMemoryAsset(byte[] data, string type)
        {
            //Check
            if (data == null || type == null)
                throw new ArgumentNullException();

            //Set
            this.data = data;
            this.type = type;

            //Allocate handle on data
            dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);

            //Convert the type to unmnaged memory
            typeHandle = Marshal.StringToHGlobalAnsi(this.type);
        }

        private readonly byte[] data;
        private readonly string type;
        private readonly GCHandle dataHandle;
        private readonly IntPtr typeHandle;

        private bool disposed = false;

        public static RayMemoryAsset FromStream(Stream stream, string type)
        {
            byte[] data = new byte[stream.Length];
            if (stream.Read(data, 0, data.Length) != data.Length)
                throw new Exception("Failed to read stream data entirely.");
            return new RayMemoryAsset(data, type);
        }

        public static RayMemoryAsset FromResource(string name, string type)
        {
            Assembly asm = Assembly.GetCallingAssembly();
            RayMemoryAsset asset;
            using (Stream content = asm.GetManifestResourceStream(name))
            {
                if (content == null)
                    throw new Exception($"Failed to load embedded resource with name \"{name}\" from assembly \"{asm.FullName}\"");
                asset = FromStream(content, type);
            }
            return asset;
        }

        private unsafe sbyte* TypePtr
        {
            get
            {
                if (disposed)
                    throw new ObjectDisposedException(GetType().FullName);
                return (sbyte*)typeHandle;
            }
        }

        private unsafe byte* DataPtr
        {
            get
            {
                if (disposed)
                    throw new ObjectDisposedException(GetType().FullName);
                return (byte*)dataHandle.AddrOfPinnedObject();
            }
        }

        public unsafe Font LoadAsFont(int size)
        {
            return Raylib.LoadFontFromMemory(TypePtr, DataPtr, data.Length, size, null, 0);
        }

        public unsafe Image LoadAsImage()
        {
            return Raylib.LoadImageFromMemory(TypePtr, DataPtr, data.Length);
        }

        public void Dispose()
        {
            //Check
            if (disposed)
                return;

            //Free data
            dataHandle.Free();

            //Free type
            Marshal.FreeHGlobal(typeHandle);

            //Set flag
            disposed = true;
        }
    }
}
