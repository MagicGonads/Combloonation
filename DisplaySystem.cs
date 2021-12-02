﻿using BTD_Mod_Helper.Extensions;
using System.Collections.Generic;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Simulation.Bloons;
using System.Linq;
using Assets.Scripts.Unity.Bridge;
using Assets.Scripts.Unity.Display;
using Assets.Scripts.Models.Bloons;
using System;
using UnityEngine;
using static Combloonation.Labloontory;
using static Combloonation.Helpers;
using MelonLoader;

namespace Combloonation
{
    public static class DisplaySystem
    {
        public static Dictionary<string, Texture2D> computedTextures = new Dictionary<string, Texture2D>();
        public static IOverlay emptyColor = new DelegateOverlay((c, x, y, r) => c);
        public static IOverlay boundaryColor = emptyColor;
        public static IOverlay fortifiedColorA = new ColorOverlay(HexColor("cc4c10"));
        public static IOverlay fortifiedColorB = new ColorOverlay(HexColor("cecece"));
        public static List<Tuple<IOverlay, float>> fortifiedColors = new List<Tuple<IOverlay, float>>
        {
            new Tuple<IOverlay,float>(emptyColor,30f),
            new Tuple<IOverlay,float>(fortifiedColorB,2f),
            new Tuple<IOverlay,float>(fortifiedColorA,8f),
            new Tuple<IOverlay,float>(fortifiedColorB,2f),
            new Tuple<IOverlay,float>(emptyColor,30f),
            new Tuple<IOverlay,float>(fortifiedColorB,2f),
            new Tuple<IOverlay,float>(fortifiedColorA,8f),
            new Tuple<IOverlay,float>(fortifiedColorB,2f),
            new Tuple<IOverlay,float>(emptyColor,30f)
        };
        public static IOverlay fortifiedColor = new DelegateOverlay((_c, _x, _y, _r) =>
                fortifiedColors.Select(c=>c.Item1).ToList().SplitRange(fortifiedColors.Select(c=>c.Item2).ToArray(), true, null, RegionScalarMap.Regions.vertical(_r.x,_r.x + _r.width, _r.y, _r.y + _r.height), _x + _r.x, _y + _r.y).Pixel(_c, _x, _y, _r));
        public static Dictionary<string, IOverlay> baseColors = new Dictionary<string, IOverlay>()
        {
            { "Red",     new ColorOverlay(HexColor("fe2020")) },
            { "Blue",    new ColorOverlay(HexColor("2f9ae0")) },
            { "Green",   new ColorOverlay(HexColor("78a911")) },
            { "Yellow",  new ColorOverlay(HexColor("ffd511")) },
            { "Pink",    new ColorOverlay(HexColor("f05363")) },
            { "White",   new ColorOverlay(HexColor("e7e7e7")) },
            { "Black",   new ColorOverlay(HexColor("252525")) },
            { "Lead",    new ColorOverlay(HexColor("7d85d7")) },
            { "Purple",  new ColorOverlay(HexColor("9326e0")) },
            { "Zebra",   new ColorOverlay(HexColor("bfbfbf")) },
            { "Rainbow", new ColorOverlay(HexColor("ffac24")) },
            { "Ceramic", new ColorOverlay(HexColor("bd6b1c")) },
            { "Moab",    new ColorOverlay(HexColor("1d83d9")) },
            { "Bfb",     new ColorOverlay(HexColor("ab0000")) },
            { "Zomg",    new ColorOverlay(HexColor("cefc02")) },
            { "Ddt",     new ColorOverlay(HexColor("454b41")) },
            { "Bad",     new ColorOverlay(HexColor("bb00c6")) },
        };
        public static Dictionary<string, IOverlay> propColors = new Dictionary<string, IOverlay>()
        {
            { "Camo",       new ColorOverlay(HexColor("000000")) },
            { "Fortified",  new ColorOverlay(HexColor("ff8f20")) },
        };

        public interface IOverlay
        {
            Color Pixel(Color c, int x, int y, Rect r);
        }

        public class DelegateOverlay : IOverlay
        {
            public Func<Color, int, int, Rect, Color> func;

            public DelegateOverlay(Func<Color, int, int, Rect, Color> func)
            {
                this.func = func;
            }
            public Color Pixel(Color c, int x, int y, Rect r)
            {
                return func(c, x, y, r);
            }
        }

        public class PipeOverlay : IOverlay
        {
            public IOverlay a;
            public IOverlay b;

            public PipeOverlay(IOverlay a, IOverlay b)
            {
                this.a = a; this.b = b;
            }
            public Color Pixel(Color c, int x, int y, Rect r)
            {
                var _c = a.Pixel(c, x, y, r);
                return b.Pixel(_c, x, y, r);
            }
        }

        public class GrayscaleOverlay : IOverlay
        {
            public IOverlay c;
            public GrayscaleOverlay(IOverlay c)
            {
                this.c = c;
            }

            public Color Pixel(Color c, int x, int y, Rect r)
            {
                var _c = this.c.Pixel(c,x,y,r);
                var t = _c.grayscale;
                return new Color(t, t, t, _c.a);
            }
        }

        public class ValueInvertOverlay : IOverlay
        {
            public IOverlay c;
            public ValueInvertOverlay(IOverlay c)
            {
                this.c = c;
            }

            public Color Pixel(Color c, int x, int y, Rect r)
            {
                var _c = this.c.Pixel(c,x,y,r);
                return new Color(1-_c.r,1-_c.g,1-_c.b,_c.a);
            }
        }

        public class TintOverlay : IOverlay
        {
            public float t = 0.8f;
            public Func<float, float, Rect, float> tf;
            public IOverlay c;

            public TintOverlay(IOverlay c)
            {
                this.c = c;
            }
            public TintOverlay(IOverlay c, float t) : this(c)
            {
                this.t = t;
            }
            public TintOverlay(IOverlay c, Func<float, float, Rect, float> tf) : this(c)
            {
                this.tf = tf;
            }

            public Color Pixel(Color mc, int x, int y, Rect r)
            {
                var tp = t;
                if (tf != null)
                {
                    var _x = x + r.x; var _y = y + r.y;
                    tp = tf(_x, _y, r);
                }
                var tc = c.Pixel(mc, x, y, r);
                //Color.RGBToHSV(mc, out var mh, out var ms, out var mv);
                //Color.RGBToHSV(tc, out var th, out var ts, out var tv);
                //var col = Color.HSVToRGB(th, ms, mv);
                //col.a = mc.a;
                //return Color.Lerp(col, new Color(tc.r, tc.g, tc.b, mc.a), tp
                return Color.Lerp(mc, new Color(tc.r, tc.g, tc.b, mc.a), tp);
            }
        }

        public class ColorOverlay : IOverlay
        {

            public Color c;
            public ColorOverlay(Color c)
            {
                this.c = c;
            }
            public Color Pixel(Color c, int x, int y, Rect r)
            {
                return new Color(this.c.r, this.c.g, this.c.b, c.a);
            }
        }

        public class BoundOverlay : IOverlay
        {

            public IOverlay ci;
            public IOverlay co;
            public float b = 1f;
            public Func<float,float,Rect,bool> bf;

            public BoundOverlay(IOverlay ci, IOverlay co)
            {
                this.ci = ci;
                this.co = co;
            }

            public BoundOverlay(IOverlay ci, IOverlay co, float b) : this(ci, co)
            {
                this.b = b;
            }

            public BoundOverlay(IOverlay ci, IOverlay co, Func<float,float,Rect,bool> bf) : this(ci, co)
            {
                this.bf = bf;
            }
                            

            public Color Pixel(Color c, int x, int y, Rect r)
            {
                bool ins;
                var _x = x + r.x; var _y = y + r.y;
                if (bf == null)
                {
                    ins = _x * _x + _y * _y > b * b;
                }
                else ins = bf(_x, _y, r);
                if (ins)
                {
                    return co.Pixel(c, x, y, r);
                }
                return ci.Pixel(c, x, y, r);
            }
        }

        public static Color HexColor(string hex)
        {
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color32(r, g, b, 255);
        }

        public static Color Average(this Color a, Color b)
        {
            return Color.Lerp(a, b, (1 + b.a - a.a) / 2);
        }

        public static Tuple<IOverlay, List<IOverlay>> GetColors(this BloonModel bloon)
        {
            var ids = BaseBloonNamesFromName(bloon.name);
            var primary = ids.First();
            var got = baseColors.TryGetValue(primary, out var pcol);
            if (!got) pcol = emptyColor;
            var cols = new List<IOverlay> { };
            foreach (var id in ids.Skip(1))
            {
                got = baseColors.TryGetValue(id, out var col);
                if (got) cols.Add(col);
            }
            return new Tuple<IOverlay, List<IOverlay>>(pcol, cols);
        }

        public static List<IOverlay> GetSecondaryColors(this BloonModel bloon)
        {
            var cols = new List<IOverlay> { };
            foreach (var id in BaseBloonNamesFromName(bloon.name).Skip(1))
            {
                var got = baseColors.TryGetValue(id, out var col);
                if (got) cols.Add(col);
            }
            return cols;
        }
        public static IOverlay GetPrimaryColor(this BloonModel bloon)
        {
            var primary = BaseBloonNamesFromName(bloon.name).First();
            var got = baseColors.TryGetValue(primary, out var col);
            if (got) return col;
            return emptyColor;

        }

        public static IEnumerable<Tuple<int, int>> GetEnumerator(this Texture2D texture)
        {
            for (int x = 0; x < texture.width; x++) for (int y = 0; y < texture.height; y++)
            {
                yield return new Tuple<int, int>(x, y);
            }
        }

        public static Texture2D Duplicate(this Texture texture, Rect? proj = null)
        {
            if (proj == null) proj = new Rect(0, 0, texture.width, texture.height);
            var rect = (Rect)proj;
            texture.filterMode = FilterMode.Point;
            RenderTexture rt = RenderTexture.GetTemporary(texture.width, texture.height);
            rt.filterMode = FilterMode.Point;
            RenderTexture.active = rt;
            Graphics.Blit(texture, rt);
            Texture2D texture2 = new Texture2D((int)rect.width, (int)rect.height);
            texture2.ReadPixels(new Rect(rect.x, texture.height - rect.height - rect.y, rect.width, rect.height), 0, 0);
            texture2.Apply();
            RenderTexture.active = null;
            return texture2;
        }

        public static Texture2D ToTexture2D(this Texture texture)
        {
            return (texture is Texture2D t2D) ? t2D : texture.Duplicate();
        }

        public static Texture2D ToReadable(this Texture texture)
        {
            var t2D = texture.ToTexture2D();
            return (t2D.isReadable) ? t2D : t2D.Duplicate();
        }

        public static IEnumerable<Tuple<int, int>> GetEnumerator(this Texture texture)
        {
            return texture.ToTexture2D().GetEnumerator();
        }

        public static Texture2D Duplicate(this Texture texture, Func<int, int, Color, Color> func, Rect? proj = null)
        {
            if (proj == null)
            {
                proj = new Rect(0, 0, texture.width, texture.height);
            }
            var t = texture.Duplicate(proj);
            foreach (var xy in t.GetEnumerator())
            {
                var x = xy.Item1; var y = xy.Item2;
                t.SetPixel(x, y, func(x, y, t.GetPixel(x, y)));
            }
            t.Apply();
            return t;
        }

        public static Rect RectOrTexture(Texture texture, Rect? proj = null)
        {
            float w; float h; float x; float y;
            if (proj is Rect rect) {
                w = rect.width; h = rect.height;
                x = rect.x; y = rect.y;
            }
            else {
                w = texture.width; h = texture.height;
                x = 0f; y = 0f;
            }
            return new Rect(x, y, w, h);
        }

        public static Tuple<RegionScalarMap, Rect> GetRegionMap(Texture texture, Rect? proj = null)
        {
            int w; int h;
            if (proj is Rect rect) { w = (int)rect.width; h = (int)rect.height; }
            else { w = texture.width; h = texture.height; }
            var w2 = w / 2; var h2 = h / 2;
            var map = RegionScalarMap.Regions.spiral(1.3f, 0.6f)(-w2, w - w2, -h2, h - h2);
            return new Tuple<RegionScalarMap, Rect>(map, new Rect(-w2, -h2, w, h));
        }

        public static Texture2D NewMergedTexture(this FusionBloonModel bloon, Texture texture, Rect? proj = null)
        {
            if (bloon == null) throw new ArgumentNullException(nameof(bloon));
            var cols = GetColors(bloon);
            if (cols.Item2.Count == 0) return texture.Duplicate(proj);
            var ws = bloon.fusands.Skip(1).Where(b => baseColors.ContainsKey(b.baseId)).Select(b => b.danger).ToArray();
            var map = GetRegionMap(texture, proj);
            var mrect = map.Item2;
            var r = Math.Min(mrect.width, mrect.height)/2;
            var fbase = bloon.fusands.First();
            r *= ws[0] / fbase.danger;
            var dx = 0f; var dy = 0f;
            if (fbase.isMoab)
            {
                dx = mrect.width * 0.165f;
                dy = mrect.height * 0.1f;
                r *= 0.5f;
            }
            else if (!fbase.isGrow)
            {
                dy = -mrect.height * 0.05f;
            }
            float r_iob, r_iib, r_oob;
            Func<float, float, float> curve;
            if (!fbase.isGrow && bloon.isGrow)
            {
                curve = (x, y) => (float)HeartCurve(x, y);
                r *= 0.90f;
            }
            else
            {
                curve = (x, y) => (float)CircleCurve(x, y);
                r *= 1.25f;
            }
            IOverlay fcol = emptyColor;
            if (!fbase.isFortified && bloon.isFortified)
            {
                fcol = fortifiedColor;
            }
            r_iob = r*0.6f; r_iib = 0.85f*r_iob; r_oob = r_iob * 1.15f;
            Func<float,float,Rect,float> tf = (x,y,_r) => (float)TERF(curve(x/r_oob,y/r_oob),1f,-1f);
            var tcols = cols.Item2;
            var dcol = new DelegateOverlay((_c, _x, _y, _r) =>
                tcols.SplitRange(ws, true, null, map.Item1, _x + _r.x, _y + _r.y).Pixel(_c, _x, _y, _r));
            var bcol = new BoundOverlay(dcol, boundaryColor, (x, y, _r) => curve(x / r_iib, y / r_iib) >= 0);
            var bbcol = new BoundOverlay(bcol, dcol, (x,y,_r) => curve(x/r_iob,y/r_iob) >= 0);
            var bbbcol = new BoundOverlay(new TintOverlay(bbcol, tf), emptyColor, (x, y, _r) => curve(x / r_oob, y / r_oob) >= 0);
            var col = new PipeOverlay(fcol, bbbcol);
            return texture.Duplicate((x, y, c) => col.Pixel(c, x + (int)dx, y + (int)dy, mrect), proj);
        }

        public static Texture2D GetMergedTexture(this FusionBloonModel bloon, Texture oldTexture, Rect? proj = null)
        {
            if (bloon == null) throw new ArgumentNullException(nameof(bloon));
            if (oldTexture == null) return computedTextures[bloon.name] = null;
            if (oldTexture.isReadable) return null;
            var exists = computedTextures.TryGetValue(bloon.name, out var texture);
            if (exists) return texture;
            computedTextures[bloon.name] = texture = bloon.NewMergedTexture(oldTexture, proj);
            if (texture != null) texture.SaveToPNG($"{Main.folderPath}/{DebugString(bloon.name)}.png");
            return texture;
        }

        public static void SetBloonAppearance(this FusionBloonModel bloon, UnityDisplayNode graphic)
        {
            var sprite = graphic.sprite;
            if (sprite != null)
            {
                var texture = bloon.GetMergedTexture(sprite.sprite.texture, sprite.sprite.textureRect);
                if (texture != null)
                {
                    sprite.sprite = texture.CreateSpriteFromTexture(sprite.sprite.pixelsPerUnit);
                }
            }
            else
            {
                var renderer = graphic.genericRenderers.First(r => r.name == "Body");
                var texture = bloon.GetMergedTexture(renderer.material.mainTexture);
                if (texture != null) foreach (var r in graphic.genericRenderers.Where(r => r.name == "Body")) r.SetMainTexture(texture);
            }
        }

        public static void SetBloonAppearance(Bloon bloon)
        {
            var graphic = bloon?.display?.node?.graphic;
            if (graphic == null) return;
            if (GetBloonByName(bloon.bloonModel.name) is FusionBloonModel fusion) SetBloonAppearance(fusion, graphic);
        }

        public static void OnInGameUpdate(InGame inGame)
        {
            List<BloonToSimulation> bloonSims;
            try { bloonSims = inGame.bridge.GetAllBloons().ToList(); } catch { return; }
            foreach (var bloonSim in bloonSims) { SetBloonAppearance(bloonSim.GetBloon()); }
        }

    }
}
