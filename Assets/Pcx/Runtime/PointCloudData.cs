// Pcx - Point cloud importer & renderer for Unity
// https://github.com/keijiro/Pcx

using UnityEngine;
using System.Collections.Generic;
using System;

namespace Pcx
{
    /// A container class optimized for compute buffer.
    public sealed class PointCloudData : ScriptableObject
    {
        #region Public properties

        /// Byte size of the point element.
        public const int elementSize = sizeof(float) * 4;

        /// Number of points.
        public int pointCount {
            get { return _pointData.Length; }
        }

        /// Get access to the compute buffer that contains the point cloud.
        public ComputeBuffer computeBuffer {
            get {
                if (_pointBuffer == null)
                {
                    _pointBuffer = new ComputeBuffer(pointCount, elementSize);
                    _pointBuffer.SetData(_pointData);
                }
                return _pointBuffer;
            }
        }

        /// Get access to the compute buffer after points have changed (but number of points remains constant).
        public ComputeBuffer recomputeBuffer
        {
            get
            {
                _pointBuffer.SetData(_pointData);
                return _pointBuffer;
            }
        }

        #endregion

        #region ScriptableObject implementation

        ComputeBuffer _pointBuffer;

        void OnDisable()
        {
            if (_pointBuffer != null)
            {
                _pointBuffer.Release();
                _pointBuffer = null;
            }
        }

        #endregion

        #region Serialized data members

        [System.Serializable]
        struct Point
        {
            public Vector3 position;
            public uint color;
        }

        [SerializeField] Point[] _pointData;

        #endregion

        #region Editor functions

        #if UNITY_EDITOR

        static uint EncodeColor(Color c)
        {
            const float kMaxBrightness = 16;

            var y = Mathf.Max(Mathf.Max(c.r, c.g), c.b);
            y = Mathf.Clamp(Mathf.Ceil(y * 255 / kMaxBrightness), 1, 255);

            var rgb = new Vector3(c.r, c.g, c.b);
            rgb *= 255 * 255 / (y * kMaxBrightness);

            return ((uint)rgb.x      ) |
                   ((uint)rgb.y <<  8) |
                   ((uint)rgb.z << 16) |
                   ((uint)y     << 24);
        }

        public void Initialize(List<Vector3> positions, List<Color32> colors)
        {
            _pointData = new Point[positions.Count];
            for (var i = 0; i < _pointData.Length; i++)
            {
                _pointData[i] = new Point
                {
                    position = positions[i],
                    color = EncodeColor(colors[i])
                };
            }
        }

        public void SegmentPointCloud(Vector3 corner1, Vector3 corner2)
        {
            float[] min_max = new float[6];

            min_max[0] = _pointData[0].position.x;
            min_max[1] = _pointData[0].position.x;
            min_max[2] = _pointData[0].position.y;
            min_max[3] = _pointData[0].position.y;
            min_max[4] = _pointData[0].position.z;
            min_max[5] = _pointData[0].position.z;

            int count_red = 0;
            int count_black = 0;


            for (var i = 0; i < _pointData.Length; i++)
            {
                _pointData[i].color = EncodeColor(new Color32(255, 255, 255, 255));
                if (_pointData[i].position.x > min_max[0])
                {
                    min_max[0] = _pointData[i].position.x;
                }
                if (_pointData[i].position.x < min_max[1])
                {
                    min_max[1] = _pointData[i].position.x;
                }
                if (_pointData[i].position.x > min_max[2])
                {
                    min_max[2] = _pointData[i].position.y;
                }
                if (_pointData[i].position.x < min_max[3])
                {
                    min_max[3] = _pointData[i].position.y;
                }
                if (_pointData[i].position.x > min_max[4])
                {
                    min_max[4] = _pointData[i].position.z;
                }
                if (_pointData[i].position.x < min_max[5])
                {
                    min_max[5] = _pointData[i].position.z;
                }

                if (_pointData[i].position.x > corner1.x &&
                    _pointData[i].position.y > corner1.y &&
                    _pointData[i].position.z > corner1.z &&
                    _pointData[i].position.x < corner2.x &&
                    _pointData[i].position.y < corner2.y &&
                    _pointData[i].position.z < corner2.z)
                {
                    count_red++;
                    _pointData[i].color = EncodeColor(new Color32(255, 0, 255, 255));
                }

            }
            if (_pointBuffer == null)
            {
                _pointBuffer = new ComputeBuffer(pointCount, elementSize);
            }

            //Debug.Log(corner1);
            //Debug.Log(corner2);
            //_pointBuffer.SetData(_pointData);

        }

#endif

        #endregion
    }
}
