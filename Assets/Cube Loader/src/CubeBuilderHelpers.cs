﻿using UnityEngine;

namespace Assets.Cube_Loader.src
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    public static class CubeBuilderHelpers
    {
        /* OBJ file tags */
        private const string O = "o";
        private const string G = "g";
        private const string V = "v";
        private const string VT = "vt";
        private const string VN = "vn";
        private const string F = "f";
        private const string MTL = "mtllib";
        private const string UML = "usemtl";

        public static void SetDefaultMaterialData(List<MaterialData> materialDatas, int x, int y)
        {
            MaterialData current = new MaterialData();

            // newmtl material_0
            current = new MaterialData();
            current.x = x;
            current.y = y;
            current.name = "material_" + x + "_" + y;
            materialDatas.Add(current);

            // Ka 0.200000 0.200000 0.200000
            current.ambient = gc(new[] { "Ka", "0.200000", "0.200000", "0.200000" });

            // Kd 0.800000 0.800000 0.800000
            current.diffuse = gc(new[] { "Kd", "0.800000", "0.800000", "0.800000" });

            // Ks 1.000000 1.000000 1.000000
            current.specular = gc(new string[] { "Ks", "1.000000", "1.000000", "1.000000" });

            // Tr 1.000000
            current.alpha = cf("1.000000");

            // illum 2
            current.illumType = ci("2");

            // Ns 0.000000
            current.shininess = cf("0.000000") / 1000;

            // map_Kd model.jpg
            current.diffuseTexPath = "model.jpg";
        }

        private static float cf(string v)
        {
            return Convert.ToSingle(v.Trim(), new CultureInfo("en-US"));
        }

        private static int ci(string v)
        {
            return Convert.ToInt32(v.Trim(), new CultureInfo("en-US"));
        }

        private static Color gc(string[] p)
        {
            return new Color(cf(p[1]), cf(p[2]), cf(p[3]));
        }

        public static Material[] GetMaterial(bool useUnlitShader, MaterialData md)
        {
            Material[] m;
            // Use an unlit shader for the model if set
            if (useUnlitShader)
            {
                m = new Material[] { new Material(Shader.Find(("Unlit/Texture"))) };
            }
            else
            {
                if (md.illumType == 2)
                {
                    m = new Material[] { new Material(Shader.Find("Specular")) };
                    m[0].SetColor("_SpecColor", md.specular);
                    m[0].SetFloat("_Shininess", md.shininess);
                }
                else
                {
                    m = new Material[] { new Material(Shader.Find("Diffuse")) };
                }

                m[0].SetColor("_Color", md.diffuse);
            }

            if (md.diffuseTex != null)
                m[0].SetTexture("_MainTex", md.diffuseTex);

            return m;
        }

        public static void SetGeometryData(string data, GeometryBuffer buffer)
        {
            string[] lines = data.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i++)
            {
                string l = lines[i].Trim();

                if (l.IndexOf("#") != -1)
                    l = l.Substring(0, l.IndexOf("#"));
                string[] p = l.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (p.Length > 1)
                {
                    switch (p[0])
                    {
                        case O:
                            buffer.PushObject(p[1].Trim());
                            break;
                        case G:
                            buffer.PushGroup(p[1].Trim());
                            break;
                        case V:
                            buffer.PushVertex(new Vector3(
                                CubeBuilderHelpers.cf(p[1]),
                                CubeBuilderHelpers.cf(p[2]),
                                CubeBuilderHelpers.cf(p[3]))
                                );
                            break;
                        case VT:
                            buffer.PushUV(new Vector2(CubeBuilderHelpers.cf(p[1]), CubeBuilderHelpers.cf(p[2])));
                            break;
                        case VN:
                            buffer.PushNormal(new Vector3(CubeBuilderHelpers.cf(p[1]), CubeBuilderHelpers.cf(p[2]), CubeBuilderHelpers.cf(p[3])));
                            break;
                        case F:
                            for (int j = 1; j < p.Length; j++)
                            {
                                string[] c = p[j].Trim().Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                FaceIndices fi = new FaceIndices();
                                fi.vi = CubeBuilderHelpers.ci(c[0]) - 1;
                                if (c.Length > 1)
                                {
                                    fi.vu = CubeBuilderHelpers.ci(c[1]) - 1;
                                }
                                if (c.Length > 2)
                                {
                                    fi.vn = CubeBuilderHelpers.ci(c[2]) - 1;
                                }
                                buffer.PushFace(fi);
                            }
                            break;
                        case MTL:
                            // mtllib = p[1].Trim();
                            break;
                        case UML:
                            buffer.PushMaterialName(p[1].Trim());
                            break;
                    }
                }
            }
            // buffer.Trace();
        }
    }
}
