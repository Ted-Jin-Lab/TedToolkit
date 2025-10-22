using System;
using System.Drawing;
using GH_IO.Serialization;
using GH_IO.Types;

namespace TedToolkit.Grasshopper;

internal static class IoHelper
{
    #region Write

    public static void Write(GH_IWriter writer, string key, bool value)
    {
        writer.SetBoolean(key, value);
    }

    public static void Write(GH_IWriter writer, string key, byte value)
    {
        writer.SetByte(key, value);
    }

    public static void Write(GH_IWriter writer, string key, int value)
    {
        writer.SetInt32(key, value);
    }

    public static void Write(GH_IWriter writer, string key, long value)
    {
        writer.SetInt64(key, value);
    }

    public static void Write(GH_IWriter writer, string key, float value)
    {
        writer.SetSingle(key, value);
    }

    public static void Write(GH_IWriter writer, string key, double value)
    {
        writer.SetDouble(key, value);
    }

    public static void Write(GH_IWriter writer, string key, decimal value)
    {
        writer.SetDecimal(key, value);
    }

    public static void Write(GH_IWriter writer, string key, DateTime value)
    {
        writer.SetDate(key, value);
    }

    public static void Write(GH_IWriter writer, string key, Guid value)
    {
        writer.SetGuid(key, value);
    }

    public static void Write(GH_IWriter writer, string key, string value)
    {
        writer.SetString(key, value);
    }

    public static void Write(GH_IWriter writer, string key, Point value)
    {
        writer.SetDrawingPoint(key, value);
    }

    public static void Write(GH_IWriter writer, string key, PointF value)
    {
        writer.SetDrawingPointF(key, value);
    }

    public static void Write(GH_IWriter writer, string key, Size value)
    {
        writer.SetDrawingSize(key, value);
    }

    public static void Write(GH_IWriter writer, string key, SizeF value)
    {
        writer.SetDrawingSizeF(key, value);
    }

    public static void Write(GH_IWriter writer, string key, Rectangle value)
    {
        writer.SetDrawingRectangle(key, value);
    }

    public static void Write(GH_IWriter writer, string key, RectangleF value)
    {
        writer.SetDrawingRectangleF(key, value);
    }

    public static void Write(GH_IWriter writer, string key, Color value)
    {
        writer.SetDrawingColor(key, value);
    }

    public static void Write(GH_IWriter writer, string key, Bitmap value)
    {
        writer.SetDrawingBitmap(key, value);
    }

    public static void Write(GH_IWriter writer, string key, byte[] value)
    {
        writer.SetByteArray(key, value);
    }

    public static void Write(GH_IWriter writer, string key, double[] value)
    {
        writer.SetDoubleArray(key, value);
    }

    public static void Write(GH_IWriter writer, string key, GH_Point2D value)
    {
        writer.SetPoint2D(key, value);
    }

    public static void Write(GH_IWriter writer, string key, GH_Point3D value)
    {
        writer.SetPoint3D(key, value);
    }

    public static void Write(GH_IWriter writer, string key, GH_Point4D value)
    {
        writer.SetPoint4D(key, value);
    }

    public static void Write(GH_IWriter writer, string key, GH_Interval1D value)
    {
        writer.SetInterval1D(key, value);
    }

    public static void Write(GH_IWriter writer, string key, GH_Interval2D value)
    {
        writer.SetInterval2D(key, value);
    }

    public static void Write(GH_IWriter writer, string key, GH_Line value)
    {
        writer.SetLine(key, value);
    }

    public static void Write(GH_IWriter writer, string key, GH_BoundingBox value)
    {
        writer.SetBoundingBox(key, value);
    }

    public static void Write(GH_IWriter writer, string key, GH_Plane value)
    {
        writer.SetPlane(key, value);
    }

    public static void Write(GH_IWriter writer, string key, GH_Version value)
    {
        writer.SetVersion(key, value);
    }

    #endregion

    #region Read

    public static bool Read(GH_IReader reader, string key, ref bool value)
    {
        return reader.TryGetBoolean(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref byte value)
    {
        return reader.TryGetByte(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref int value)
    {
        return reader.TryGetInt32(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref long value)
    {
        return reader.TryGetInt64(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref float value)
    {
        return reader.TryGetSingle(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref double value)
    {
        return reader.TryGetDouble(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref decimal value)
    {
        return reader.TryGetDecimal(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref DateTime value)
    {
        return reader.TryGetDate(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref Guid value)
    {
        return reader.TryGetGuid(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref string value)
    {
        return reader.TryGetString(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref Point value)
    {
        return reader.TryGetDrawingPoint(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref PointF value)
    {
        return reader.TryGetDrawingPointF(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref Size value)
    {
        return reader.TryGetDrawingSize(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref SizeF value)
    {
        return reader.TryGetDrawingSizeF(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref Rectangle value)
    {
        return reader.TryGetDrawingRectangle(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref RectangleF value)
    {
        return reader.TryGetDrawingRectangleF(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref Color value)
    {
        return reader.TryGetDrawingColor(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref GH_Point2D value)
    {
        return reader.TryGetPoint2D(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref GH_Point3D value)
    {
        return reader.TryGetPoint3D(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref GH_Point4D value)
    {
        return reader.TryGetPoint4D(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref GH_Interval1D value)
    {
        return reader.TryGetInterval1D(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref GH_Interval2D value)
    {
        return reader.TryGetInterval2D(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref GH_Line value)
    {
        return reader.TryGetLine(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref GH_BoundingBox value)
    {
        return reader.TryGetBoundingBox(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref GH_Plane value)
    {
        return reader.TryGetPlane(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref GH_Version value)
    {
        return reader.TryGetVersion(key, ref value);
    }

    public static bool Read(GH_IReader reader, string key, ref byte[] value)
    {
        try
        {
            value = reader.GetByteArray(key);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool Read(GH_IReader reader, string key, ref double[] value)
    {
        try
        {
            value = reader.GetDoubleArray(key);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool Read(GH_IReader reader, string key, ref Bitmap value)
    {
        try
        {
            value = reader.GetDrawingBitmap(key);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}