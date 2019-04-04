﻿using System;
using System.Runtime.InteropServices;
using UnityEngine;


public class cwipc_util_pinvoke
{
    [DllImport("cwipc_util")]
    extern static IntPtr cwipc_read([MarshalAs(UnmanagedType.LPStr)]string filename, System.UInt64 timestamp, ref System.IntPtr errorMessage);
    [DllImport("cwipc_util")]
    extern static void cwipc_free(IntPtr pc);
    [DllImport("cwipc_util")]
    extern static uint cwipc_timestamp(IntPtr pc);
    [DllImport("cwipc_util")]
    extern static Int32 cwipc_get_uncompressed_size(IntPtr pc, uint dataVersion= 0x20190209);
    [DllImport("cwipc_util")]
    extern static int cwipc_copy_uncompressed(IntPtr pc, IntPtr data, Int32 size);
    [DllImport("cwipc_util")]
    extern static System.IntPtr cwipc_source_get(IntPtr src);
    [DllImport("cwipc_util")]
    extern static void cwipc_source_free(IntPtr src);
    [DllImport("cwipc_util")]
    extern static private IntPtr cwipc_synthetic();

    public static System.IntPtr GetPointCloudFromPly() {

//        System.IntPtr src = cwipc_synthetic();
//        return cwipc_source_get(src);
        System.IntPtr ptr = System.IntPtr.Zero;
        return cwipc_read(Application.streamingAssetsPath + "/pcl_frame1.ply", 0, ref ptr);
    }

    public static System.IntPtr GetPointCloudFromCWICPC(string filename)
    {
        float init = Time.realtimeSinceStartup;
        var bytes = System.IO.File.ReadAllBytes(Application.streamingAssetsPath + "/"+ filename);
        var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);
        float read = Time.realtimeSinceStartup;

        var pc = cwipc_codec_pinvoke.cwipc_decompress(ptr, bytes.Length);
        float decom1 = Time.realtimeSinceStartup;
        pc = cwipc_codec_pinvoke.cwipc_decompress(ptr, bytes.Length);
        float decom2 = Time.realtimeSinceStartup;

        Debug.Log(">>> read " + (read - init) + " decom " + (decom1 - read) + " decom2 " + (decom2 - decom1));
        

        return pc;
    }

    public static void UpdatePointBuffer(System.IntPtr pc, ref ComputeBuffer pointBuffer)
    {
        uint ts = cwipc_timestamp(pc);
        System.Int32 size = cwipc_get_uncompressed_size(pc);
        unsafe
        {
            var array = new Unity.Collections.NativeArray<byte>(size, Unity.Collections.Allocator.Temp);

            System.IntPtr ptr = (System.IntPtr)Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(array);
            int ret = cwipc_copy_uncompressed(pc, ptr, size);

            if (pointBuffer == null) pointBuffer = new ComputeBuffer(ret, sizeof(float) * 4);
            pointBuffer.SetData<byte>(array, 0, 0, size);

            array.Dispose();
        }
    }

    [StructLayout(LayoutKind.Sequential)] // Also tried with Pack=1
    public struct PointCouldVertex
    {
        public Vector3 vertex;
        public Color32 color;
    }

    public static void UpdatePointBuffer(System.IntPtr pc, ref Mesh mesh)
    {
        uint ts = cwipc_timestamp(pc);
        System.Int32 size = cwipc_get_uncompressed_size(pc);
        unsafe
        {
            var sizeT = Marshal.SizeOf(typeof(PointCouldVertex));
            var array = new Unity.Collections.NativeArray<PointCouldVertex>(size / sizeT, Unity.Collections.Allocator.Temp);
            System.IntPtr ptr = (System.IntPtr)Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(array);
            int ret = cwipc_copy_uncompressed(pc, ptr, size);

            var points = new Vector3[array.Length];
            var indices = new int[array.Length];
            var colors = new Color32[array.Length];

            for (int i = 0; i < array.Length; i++) {
                points[i] = array[i].vertex;
                indices[i] = i;
                colors[i] = array[i].color;
            }

            mesh.vertices = points;
            mesh.colors32 = colors;
            mesh.SetIndices(indices, MeshTopology.Points, 0);

            array.Dispose();
        }
    }

}
