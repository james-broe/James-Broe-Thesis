// Pcx - Point cloud importer & renderer for Unity
// https://github.com/keijiro/Pcx

using System.Collections;
using UnityEngine;

namespace Pcx
{
    /// A renderer class that renders a point cloud contained by PointCloudData.
    [ExecuteInEditMode]
    public sealed class PointCloudRenderer : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] PointCloudData _sourceData = null;

        public PointCloudData sourceData
        {
            get { return _sourceData; }
            set { _sourceData = value; }
        }

        [SerializeField] Color _pointTint = new Color(0.5f, 0.5f, 0.5f, 1);

        public Color pointTint
        {
            get { return _pointTint; }
            set { _pointTint = value; }
        }

        [SerializeField] float _pointSize = 0.05f;

        public float pointSize
        {
            get { return _pointSize; }
            set { _pointSize = value; }
        }

        [SerializeField] Vector3 _corner1;

        public Vector3 corner1
        {
            get { return _corner1; }
            set { _corner1 = value; }
        }

        [SerializeField] Vector3 _corner2;

        public Vector3 corner2
        {
            get { return _corner2; }
            set { _corner2 = value; }
        }

        #endregion

        #region Public properties (nonserialized)

        public ComputeBuffer sourceBuffer { get; set; }

        #endregion

        #region Internal resources

        [SerializeField, HideInInspector] Shader _pointShader = null;
        [SerializeField, HideInInspector] Shader _diskShader = null;

        #endregion

        #region Private objects

        Material _pointMaterial;
        Material _diskMaterial;
        bool new_segment = true;

        #endregion

        #region MonoBehaviour implementation

        void OnValidate()
        {
            //Debug.Log("This is OnValidate");
            _pointSize = Mathf.Max(0, _pointSize);
            if (!corner1.Equals(corner2))
            {//
                _sourceData.SegmentPointCloud(corner1, corner2);
                sourceData = _sourceData;
                Debug.Log("Called SegmentPointCloud");
                new_segment = true;
            }
            
        }

        private void Start()
        {
            StartCoroutine(ChangeBoundingBox());
        }

        private IEnumerator ChangeBoundingBox()
        {
            yield return new WaitForSeconds(1f);
            corner1 = new Vector3(corner1.x + 100, corner1.y + 100, corner1.z);
            corner2 = new Vector3(corner2.x + 100, corner2.y + 100, corner2.z);
            _sourceData.SegmentPointCloud(corner1, corner2);
            sourceBuffer = _sourceData.computeBuffer;
            Debug.Log("Called SegmentPointCloud");
            new_segment = true;
            Debug.Log("we're changing bounding box");
            StartCoroutine(ChangeBoundingBox());
        }

        void OnDestroy()
        {
            //Debug.Log("This is OnDestroy");
            if (_pointMaterial != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(_pointMaterial);
                    Destroy(_diskMaterial);
                }
                else
                {
                    DestroyImmediate(_pointMaterial);
                    DestroyImmediate(_diskMaterial);
                }
            }
        }

        void OnRenderObject()
        {
            //Debug.Log("This is OnRenderObject");
            // We need a source data or an externally given buffer.
            if (_sourceData == null && sourceBuffer == null) return;
            // Check the camera condition.
            var camera = Camera.current;
            if ((camera.cullingMask & (1 << gameObject.layer)) == 0) return;
            if (camera.name == "Preview Scene Camera") return;

            // TODO: Do view frustum culling here.

            // Lazy initialization
            if (_pointMaterial == null || new_segment)
            {
                //Debug.Log("This is new_segment in OnRenderObject");
                _pointMaterial = new Material(_pointShader);
                _pointMaterial.hideFlags = HideFlags.DontSave;
                _pointMaterial.EnableKeyword("_COMPUTE_BUFFER");

                _diskMaterial = new Material(_diskShader);
                _diskMaterial.hideFlags = HideFlags.DontSave;
                _diskMaterial.EnableKeyword("_COMPUTE_BUFFER");//
                //Debug.Log("Made it new segment");
            }

            // Use the external buffer if given any.
            var pointBuffer = new_segment ? _sourceData.recomputeBuffer : sourceBuffer != null ?
                sourceBuffer : _sourceData.computeBuffer;

            if (_pointSize == 0)
            {
                //Debug.Log("This is _pointSize == 0 in OnRenderObject");
                //Debug.Log("Inside _pointSize=0");
                _pointMaterial.SetPass(0);
                _pointMaterial.SetColor("_Tint", _pointTint);
                _pointMaterial.SetMatrix("_Transform", transform.localToWorldMatrix);
                _pointMaterial.SetBuffer("_PointBuffer", pointBuffer);
                #if UNITY_2019_1_OR_NEWER
                Graphics.DrawProceduralNow(MeshTopology.Points, pointBuffer.count, 1);
                #else
                Graphics.DrawProcedural(MeshTopology.Points, pointBuffer.count, 1);
                #endif
            }
            else
            {

                //Debug.Log("This is else in OnRenderObject");
                //Debug.Log("Inside _pointSize!=0");
                _diskMaterial.SetPass(0);
                _diskMaterial.SetColor("_Tint", _pointTint);
                _diskMaterial.SetMatrix("_Transform", transform.localToWorldMatrix);
                _diskMaterial.SetBuffer("_PointBuffer", pointBuffer);
                _diskMaterial.SetFloat("_PointSize", pointSize);
                #if UNITY_2019_1_OR_NEWER
                Graphics.DrawProceduralNow(MeshTopology.Points, pointBuffer.count, 1);
                #else
                Graphics.DrawProcedural(MeshTopology.Points, pointBuffer.count, 1);
                #endif
            }
            new_segment = false;
            //Debug.Log("Made it to the end");

        }

        #endregion
    }
}
