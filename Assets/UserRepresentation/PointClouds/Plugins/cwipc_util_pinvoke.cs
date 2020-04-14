﻿using System;
using System.Runtime.InteropServices;

internal class API_kernel
{
    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    internal static extern IntPtr GetModuleHandle(string lpModuleName);
    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    internal static extern int GetModuleFileName(IntPtr hModule, System.Text.StringBuilder modulePath, int nSize);
}

public class cwipc
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct encoder_params
    {
        public bool do_inter_frame;    /**< (unused in this version, must be false) do inter-frame compression */
        public int gop_size;           /**< (unused in this version, ignored) spacing of I frames for inter-frame compression */
        public float exp_factor;       /**< (unused in this version, ignored). Bounding box expansion factor for inter-frame coding */
        public int octree_bits;        /**< Octree depth: a fully populated octree will have 8**octree_bits points */
        public int jpeg_quality;       /**< JPEG encoding quality */
        public int macroblock_size;    /**< (unused in this version, ignored) macroblock size for inter-frame prediction */
        public int tilenumber;         /**< 0 for encoding full pointclouds, > 0 for selecting a single tile to encode */
        public float voxelsize;        /**< If non-zero run voxelizer with this cell size to get better tiled pointcloud */
    };
    private class _API_cwipc_util
    {
        const string myDllName = "cwipc_util";
        const System.UInt64 CWIPC_API_VERSION = 0x20190522;

        [DllImport(myDllName)]
        internal extern static IntPtr cwipc_read([MarshalAs(UnmanagedType.LPStr)]string filename, System.UInt64 timestamp, ref System.IntPtr errorMessage, System.UInt64 apiVersion = CWIPC_API_VERSION);
        [DllImport(myDllName)]
        internal extern static void cwipc_free(IntPtr pc);
        [DllImport(myDllName)]
        internal extern static UInt64 cwipc_timestamp(IntPtr pc);
        [DllImport(myDllName)]
        internal extern static int cwipc_count(IntPtr pc);
        [DllImport(myDllName)]
        internal extern static float cwipc_cellsize(IntPtr pc);
        [DllImport(myDllName)]
        internal extern static System.IntPtr cwipc_get_uncompressed_size(IntPtr pc);
        [DllImport(myDllName)]
        internal extern static int cwipc_copy_uncompressed(IntPtr pc, IntPtr data, System.IntPtr size);

        [DllImport(myDllName)]
        internal extern static System.IntPtr cwipc_source_get(IntPtr src);
        [DllImport(myDllName)]
        internal extern static bool cwipc_source_eof(IntPtr src);
        [DllImport(myDllName)]
        internal extern static bool cwipc_source_available(IntPtr src, bool available);
        [DllImport(myDllName)]
        internal extern static void cwipc_source_free(IntPtr src);

        [DllImport(myDllName)]
        internal extern static IntPtr cwipc_synthetic(ref System.IntPtr errorMessage, System.UInt64 apiVersion = CWIPC_API_VERSION);

        [DllImport(myDllName)]
        internal extern static int cwipc_tiledsource_maxtile(IntPtr src);
        [DllImport(myDllName)]
        internal extern static uint cwipc_tiledsource_get_tileinfo(IntPtr src, int tileNum, IntPtr tileinfo);

    }
    private class _API_cwipc_realsense2
    {
        const string myDllName = "cwipc_realsense2";

        const System.UInt64 CWIPC_API_VERSION = 0x20190522;
        [DllImport(myDllName)]
        internal extern static IntPtr cwipc_realsense2([MarshalAs(UnmanagedType.LPStr)]string filename, ref System.IntPtr errorMessage, System.UInt64 apiVersion = CWIPC_API_VERSION);
    }
    private class _API_cwipc_codec
    {
        const string myDllName = "cwipc_codec";
        const System.UInt64 CWIPC_API_VERSION = 0x20190522;
        public const int CWIPC_ENCODER_PARAM_VERSION = 0x20190506;

        [DllImport(myDllName)]
        internal extern static IntPtr cwipc_new_decoder(ref System.IntPtr errorMessage, System.UInt64 apiVersion = CWIPC_API_VERSION);

        [DllImport(myDllName)]
        internal extern static void cwipc_decoder_feed(IntPtr dec, IntPtr compFrame, int len);

        [DllImport(myDllName)]
        internal extern static IntPtr cwipc_new_encoder(int paramVersion, ref encoder_params encParams, ref System.IntPtr errorMessage, System.UInt64 apiVersion = CWIPC_API_VERSION);

        [DllImport(myDllName)]
        internal extern static void cwipc_encoder_free(IntPtr enc);

        [DllImport(myDllName)]
        internal extern static void cwipc_encoder_feed(IntPtr enc, IntPtr pc);

        [DllImport(myDllName)]
        internal extern static bool cwipc_encoder_available(IntPtr enc, bool wait);

        [DllImport(myDllName)]
        internal extern static System.IntPtr cwipc_encoder_get_encoded_size(IntPtr enc);

        [DllImport(myDllName)]
        internal extern static bool cwipc_encoder_copy_data(IntPtr enc, IntPtr data, System.IntPtr size);

        [DllImport(myDllName)]
        internal extern static bool cwipc_encoder_at_gop_boundary(IntPtr enc);

        [DllImport(myDllName)]
        internal extern static IntPtr cwipc_downsample(IntPtr pc, float voxelSize);

        [DllImport(myDllName)]
        internal extern static IntPtr cwipc_tilefilter(IntPtr pc, int tilenum);

    }

    public class pointcloud : BaseMemoryChunk {
        internal pointcloud(System.IntPtr _pointer): base(_pointer) {
            if (_pointer == System.IntPtr.Zero)
                throw new System.Exception("cwipc.pointcloud called with NULL pointer argument");
        }

        ~pointcloud() {
            free();
        }
        
        protected void _check()
        {
            if (pointer == System.IntPtr.Zero)
                throw new System.Exception("cwipc.pointcloud method called after free()");

        }

        protected override void onfree() {
            if( pointer != IntPtr.Zero ) _API_cwipc_util.cwipc_free(pointer);
        }

        public UInt64 timestamp()         {
            return pointer != IntPtr.Zero ? _API_cwipc_util.cwipc_timestamp(pointer):0;
        }

        public int count() {
            return pointer != IntPtr.Zero ? (int)_API_cwipc_util.cwipc_count(pointer):0;
        }

        public float cellsize() {
            return pointer != IntPtr.Zero ? _API_cwipc_util.cwipc_cellsize(pointer):0;
        }

        public int get_uncompressed_size() {
            return pointer!=IntPtr.Zero?(int)_API_cwipc_util.cwipc_get_uncompressed_size(pointer):0;
        }

        public int copy_uncompressed(System.IntPtr data, int size) {
            return pointer != IntPtr.Zero ? _API_cwipc_util.cwipc_copy_uncompressed(pointer, data, (System.IntPtr)size):0;
        }

        internal System.IntPtr _intptr() {
            return pointer;
        }
    }

    public class source : BaseMemoryChunk {
        internal source(System.IntPtr _pointer) : base(_pointer) {
            if (_pointer == System.IntPtr.Zero)
                throw new System.Exception("cwipc.source called with NULL pointer argument");
        }

        protected override void onfree() {
            if (pointer != IntPtr.Zero) _API_cwipc_util.cwipc_source_free(pointer);
        }

        protected void _check()
        {
            if (pointer == System.IntPtr.Zero)
                throw new System.Exception("cwipc.source method called after free()");

        }

        /*
        ~source() {
            free();
        }
        */
        public pointcloud get() {
            if (pointer != IntPtr.Zero) {
                IntPtr pc = _API_cwipc_util.cwipc_source_get(pointer);
                if (pc == System.IntPtr.Zero) return null;
                return new pointcloud(pc);
            }
            return null;
        }

        public bool eof() {
            return pointer != IntPtr.Zero ? _API_cwipc_util.cwipc_source_eof(pointer):false;
        }

        public bool available(bool wait) {
            return pointer != IntPtr.Zero ? _API_cwipc_util.cwipc_source_available(pointer, wait):false;
        }
    }

    public class decoder : source {
        internal decoder(System.IntPtr _obj) : base(_obj) {
            if (_obj == System.IntPtr.Zero) {
                throw new System.Exception("cwipc_decoder: constructor called with null pointer");
            }
        }

        public void feed(IntPtr compFrame, int len) {
            if (pointer != IntPtr.Zero ) _API_cwipc_codec.cwipc_decoder_feed(pointer, compFrame, len);
        }

    }


    public class encoder : source {
        internal encoder(System.IntPtr _obj):base(_obj) {
            if (pointer == System.IntPtr.Zero)
                throw new System.Exception("cwipc.pointcloud called with NULL pointer argument");
        }
        protected void _check()
        {
            if (pointer == System.IntPtr.Zero)
                throw new System.Exception("cwipc.encoder method called after free()");

        }

        /*
                ~encoder() {
                    free();
                }
        */
        public void feed(pointcloud pc) {
            if (pointer != IntPtr.Zero) _API_cwipc_codec.cwipc_encoder_feed(pointer, pc.pointer);
        }

        new public bool available(bool wait) {
            return pointer != IntPtr.Zero?_API_cwipc_codec.cwipc_encoder_available(pointer, wait):false;
        }

        public int get_encoded_size() {
            return pointer != IntPtr.Zero?(int)_API_cwipc_codec.cwipc_encoder_get_encoded_size(pointer):0;
        }

        public bool copy_data(System.IntPtr data, int size) {
            return pointer != IntPtr.Zero ? _API_cwipc_codec.cwipc_encoder_copy_data(pointer, data, (System.IntPtr)size):false;
        }

        public bool at_gop_boundary() {
            return pointer != IntPtr.Zero ? _API_cwipc_codec.cwipc_encoder_at_gop_boundary(pointer):false;
        }

    }
        
    public static source synthetic() {
        System.IntPtr errorPtr = System.IntPtr.Zero;
        System.IntPtr rdr = _API_cwipc_util.cwipc_synthetic(ref errorPtr);
        if (rdr == System.IntPtr.Zero) {
            if (errorPtr == System.IntPtr.Zero) {
                throw new System.Exception("cwipc.synthetic: returned null without setting error message");
            }
            throw new System.Exception($"cwipc.synthetic: {System.Runtime.InteropServices.Marshal.PtrToStringAnsi(errorPtr)} ");
        }
        return new source(rdr);
    }

    public static source realsense2(string filename) {
        System.IntPtr errorPtr = System.IntPtr.Zero;
        System.IntPtr rdr = _API_cwipc_realsense2.cwipc_realsense2(filename, ref errorPtr);
        if (rdr == System.IntPtr.Zero) {
            if (errorPtr == System.IntPtr.Zero) {
                throw new System.Exception("cwipc.realsense2: returned null without setting error message");
            }
            throw new System.Exception($"cwipc.realsense2: {System.Runtime.InteropServices.Marshal.PtrToStringAnsi(errorPtr)} ");
        }
        return new source(rdr);
    }

    public static decoder new_decoder() {
        System.IntPtr errorPtr = System.IntPtr.Zero;
        System.IntPtr dec = _API_cwipc_codec.cwipc_new_decoder(ref errorPtr);
        if (dec == System.IntPtr.Zero) {
            if (errorPtr == System.IntPtr.Zero) {
                throw new System.Exception("cwipc.new_decoder: returned null without setting error message");
            }
            throw new System.Exception($"cwipc_new_decoder: {System.Runtime.InteropServices.Marshal.PtrToStringAnsi(errorPtr)} ");
        }
        return new decoder(dec);

    }

    public static encoder new_encoder(encoder_params par) {
        System.IntPtr errorPtr = System.IntPtr.Zero;
        System.IntPtr enc = _API_cwipc_codec.cwipc_new_encoder(_API_cwipc_codec.CWIPC_ENCODER_PARAM_VERSION, ref par, ref errorPtr);
        if (enc == System.IntPtr.Zero) {
            if (errorPtr == System.IntPtr.Zero) {
                throw new System.Exception("cwipc.new_encoder: returned null without setting error message");
            }
            throw new System.Exception($"cwipc_new_encoder: {System.Runtime.InteropServices.Marshal.PtrToStringAnsi(errorPtr)} ");
        }
        return new encoder(enc);

    }

    public static pointcloud downsample(pointcloud pc, float voxelSize) {
        System.IntPtr pcPtr = pc._intptr();
        System.IntPtr rvPtr = _API_cwipc_codec.cwipc_downsample(pcPtr, voxelSize);
        if (rvPtr == System.IntPtr.Zero) return null;
        return new pointcloud(rvPtr);
    }

    public static pointcloud tilefilter(pointcloud pc, int tileNum) {
        System.IntPtr pcPtr = pc._intptr();
        System.IntPtr rvPtr = _API_cwipc_codec.cwipc_tilefilter(pcPtr, tileNum);
        if (rvPtr == System.IntPtr.Zero) return null;
        return new pointcloud(rvPtr);
    }
}
