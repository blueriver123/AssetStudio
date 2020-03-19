using System.Collections.Generic;

namespace AssetStudio
{
    public class UnityTexEnv
    {
        public PPtr<Texture> m_Texture;
        public Vector2 m_Scale;
        public Vector2 m_Offset;

        public UnityTexEnv(ObjectReader reader)
        {
            m_Texture = new PPtr<Texture>(reader);
            m_Scale = reader.ReadVector2();
            m_Offset = reader.ReadVector2();
        }
    }

    public class UnityPropertySheet
    {
        public KeyValuePair<string, UnityTexEnv>[] m_TexEnvs;
        public KeyValuePair<string, float>[] m_Floats;
        public KeyValuePair<string, Color>[] m_Colors;

        public UnityPropertySheet(ObjectReader reader)
        {
            int m_TexEnvsSize = reader.ReadInt32();
            m_TexEnvs = new KeyValuePair<string, UnityTexEnv>[m_TexEnvsSize];
            for (int i = 0; i < m_TexEnvsSize; i++)
            {
                m_TexEnvs[i] = new KeyValuePair<string, UnityTexEnv>(reader.ReadAlignedString(), new UnityTexEnv(reader));
            }

            int m_FloatsSize = reader.ReadInt32();
            m_Floats = new KeyValuePair<string, float>[m_FloatsSize];
            for (int i = 0; i < m_FloatsSize; i++)
            {
                m_Floats[i] = new KeyValuePair<string, float>(reader.ReadAlignedString(), reader.ReadSingle());
            }

            int m_ColorsSize = reader.ReadInt32();
            m_Colors = new KeyValuePair<string, Color>[m_ColorsSize];
            for (int i = 0; i < m_ColorsSize; i++)
            {
                m_Colors[i] = new KeyValuePair<string, Color>(reader.ReadAlignedString(), reader.ReadColor4());
            }
        }
    }

    public sealed class Material : NamedObject
    {
        public PPtr<Shader> m_Shader;
        public UnityPropertySheet m_SavedProperties;
        string m_ShaderKeywords;
        uint m_LightmapFlags=0;
        int m_CustomRenderQueue=-1;
        public Material(ObjectReader reader) : base(reader)
        {
            m_Shader = new PPtr<Shader>(reader);

            if (version[0] == 4 && version[1] >= 1) //4.x
            {
                var m_ShaderKeywords = reader.ReadStringArray();
            }

            if (version[0] >= 5) //5.0 and up
            {
                m_ShaderKeywords = reader.ReadAlignedString();
                m_LightmapFlags = reader.ReadUInt32();
            }

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 6)) //5.6 and up
            {
                var m_EnableInstancingVariants = reader.ReadBoolean();
                //var m_DoubleSidedGI = a_Stream.ReadBoolean(); //2017 and up
                reader.AlignStream();
            }

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                m_CustomRenderQueue = reader.ReadInt32();
            }

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 1)) //5.1 and up
            {
                var stringTagMapSize = reader.ReadInt32();
                for (int i = 0; i < stringTagMapSize; i++)
                {
                    var first = reader.ReadAlignedString();
                    var second = reader.ReadAlignedString();
                }
            }

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 6)) //5.6 and up
            {
                var disabledShaderPasses = reader.ReadStringArray();
            }

            m_SavedProperties = new UnityPropertySheet(reader);
        }
        public string toMaterialFile()
        {
            string str = "%YAML 1.1\n% TAG !u!tag:unity3d.com,2011:\n---!u!21 & 2100000\nMaterial: \n";
            str += "  serializedVersion: 6\n  m_ObjectHideFlags: 0\n";
            str += "  m_PrefabParentObject: {fileID: 0}\n  m_PrefabInternal: { fileID: 0}\n";
            str += "  m_Name: " + m_Name + "\n";
            str += "  m_Shader: {fileID: "+ m_Shader.m_FileID.ToString()+", guid: "+m_Shader.m_PathID.ToString()+", type: unknoe\n";
            str += "  m_ShaderKeywords: " + m_ShaderKeywords+"\n";
            str += "  m_LightmapFlags: " + m_LightmapFlags.ToString() + "\n";
            str += "  m_EnableInstancingVariants: 0\n";
            str += "  m_CustomRenderQueue: " + m_CustomRenderQueue + "\n";
            str += "  stringTagMap: {}\n  disabledShaderPasses: []\n";
            str += "  m_SavedProperties:\n";
            str += "    serializedVersion: 3\n";
            str += "    m_TexEnvs:\n";
            foreach (var texEnv in m_SavedProperties.m_TexEnvs)
            {
                str += "    - "+ texEnv.Key+":\n";
                str += "        m_Texture: {fileID: 0, guid: " + texEnv.Value.m_Texture.m_PathID.ToString() + "}\n";
                str += "        m_Scale: {x: "+ texEnv.Value.m_Scale.X.ToString()+", y:"+ texEnv.Value.m_Scale.Y.ToString() + "}\n";
                str += "        m_Offset: {x: " + texEnv.Value.m_Offset.X.ToString() + ", y:" + texEnv.Value.m_Offset.Y.ToString() + "}\n";
            }
            str += "    m_Floats:\n";
            foreach (var fvar in m_SavedProperties.m_Floats)
            {
                str += "    - " + fvar.Key + ": "+fvar.Value.ToString()+"\n";
            }
            str += "    m_Colors:\n";
            foreach (var fcolor in m_SavedProperties.m_Colors)
            {
                str += "    - " + fcolor.Key + ": {r: " + fcolor.Value.R +", g: "+ fcolor.Value.G + ", b: " + fcolor.Value.B + ", a: " + fcolor.Value.A + "}\n";
            }
            return str;
        }
    }
}
