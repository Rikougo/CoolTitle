using TMPro;
using UnityEngine;

namespace Animations.Text
{
    public class WavingText : MonoBehaviour
    {
        private TextMeshProUGUI m_textGUI;

        private Vector3[] m_beforeAnimationVertices;
        private Vector3[] m_afterAnimationVertices;
        private bool m_animateText = false;

        [SerializeField] private bool m_playOnAwake = false;
        [SerializeField] private float m_unitOffset = 1.0f;
        [SerializeField] private float m_replaceDuration = 0.75f;
        private float m_replaceTiming = 0.0f;

        private void Awake()
        {
            m_textGUI = GetComponent<TextMeshProUGUI>();

            if (m_playOnAwake) PlayAnimation();
        }
        
        private void Update()
        {
            if (m_animateText)
            {
                m_textGUI.ForceMeshUpdate();
                Mesh l_mesh = m_textGUI.mesh;
                Vector3[] l_vertices = l_mesh.vertices;

                int l_floatIndex = 0;
                foreach (TMP_CharacterInfo l_info in m_textGUI.textInfo.characterInfo)
                {
                    int l_index = l_info.vertexIndex;
                    Vector3 l_offset = Wobble(Time.time + l_floatIndex);

                    l_vertices[l_index] += l_offset;
                    l_vertices[l_index + 1] += l_offset;
                    l_vertices[l_index + 2] += l_offset;
                    l_vertices[l_index + 3] += l_offset;

                    l_floatIndex++;
                }

                l_mesh.vertices = l_vertices;
                m_textGUI.canvasRenderer.SetMesh(l_mesh);
            }
            else
            {
                if (m_replaceTiming > 0.0f)
                {
                    m_replaceTiming -= Time.deltaTime;

                    float l_progress = m_replaceTiming / m_replaceDuration;

                    Mesh l_mesh = m_textGUI.mesh;
                    Vector3[] l_vertices = l_mesh.vertices;

                    if (m_replaceTiming < 0.0f)
                    {
                        l_vertices = m_beforeAnimationVertices;
                    }
                    else
                    {
                        for (int l_index = 0; l_index < l_vertices.Length; l_index++)
                        {
                            l_vertices[l_index] = Vector3.Lerp(m_beforeAnimationVertices[l_index],
                                m_afterAnimationVertices[l_index], l_progress);
                        }
                    }

                    l_mesh.vertices = l_vertices;
                    m_textGUI.canvasRenderer.SetMesh(l_mesh);
                }
            }
        }

        public void PlayAnimation()
        {
            m_animateText = true;
            m_textGUI.ForceMeshUpdate();
            m_beforeAnimationVertices = m_textGUI.mesh.vertices;
        }

        public void StopAnimation()
        {
            m_animateText = false;
            m_afterAnimationVertices = m_textGUI.mesh.vertices;
            m_replaceTiming = m_replaceDuration;
        }

        Vector2 Wobble(float p_time)
        {
            return new Vector2(0.0f, Mathf.Cos(p_time * 2.5f) * m_unitOffset);
        }
    }
}