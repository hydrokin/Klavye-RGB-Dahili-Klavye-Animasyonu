using System;
using System.Drawing;

namespace KlavyeRGB
{
    /// <summary>0..1 aralığında renk.</summary>
    public struct Rgb
    {
        public double R, G, B;

        public Rgb(double r, double g, double b) { R = r; G = g; B = b; }

        public static readonly Rgb Black = new Rgb(0, 0, 0);

        public static Rgb FromInt(int rgb)
        {
            return new Rgb(((rgb >> 16) & 255) / 255.0, ((rgb >> 8) & 255) / 255.0, (rgb & 255) / 255.0);
        }

        public static Rgb FromColor(Color c) { return new Rgb(c.R / 255.0, c.G / 255.0, c.B / 255.0); }

        public Rgb Clamp()
        {
            return new Rgb(Math.Max(0, Math.Min(1, R)), Math.Max(0, Math.Min(1, G)), Math.Max(0, Math.Min(1, B)));
        }

        public int ToInt()
        {
            Rgb c = Clamp();
            return ((int)Math.Round(c.R * 255) << 16) | ((int)Math.Round(c.G * 255) << 8) | (int)Math.Round(c.B * 255);
        }

        public Rgb Scale(double k) { return new Rgb(R * k, G * k, B * k); }

        public static Rgb Lerp(Rgb a, Rgb b, double t)
        {
            return new Rgb(a.R + (b.R - a.R) * t, a.G + (b.G - a.G) * t, a.B + (b.B - a.B) * t);
        }

        public static Rgb Hsv(double h, double s, double v)
        {
            h = (h - Math.Floor(h)) * 6.0;
            int i = (int)Math.Floor(h) % 6;
            double f = h - Math.Floor(h);
            double p = v * (1 - s), q = v * (1 - s * f), t = v * (1 - s * (1 - f));
            switch (i)
            {
                case 0: return new Rgb(v, t, p);
                case 1: return new Rgb(q, v, p);
                case 2: return new Rgb(p, v, t);
                case 3: return new Rgb(p, q, v);
                case 4: return new Rgb(t, p, v);
                default: return new Rgb(v, p, q);
            }
        }

        public void ToHsv(out double h, out double s, out double v)
        {
            double max = Math.Max(R, Math.Max(G, B)), min = Math.Min(R, Math.Min(G, B)), d = max - min;
            v = max;
            s = max <= 0 ? 0 : d / max;
            if (d <= 0) { h = 0; return; }
            if (max == R) h = ((G - B) / d) / 6.0;
            else if (max == G) h = ((B - R) / d + 2) / 6.0;
            else h = ((R - G) / d + 4) / 6.0;
            if (h < 0) h += 1;
        }

        public static string Hex(int rgb) { return "#" + (rgb & 0xFFFFFF).ToString("X6"); }
    }
}
