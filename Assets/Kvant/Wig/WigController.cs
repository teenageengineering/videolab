using UnityEngine;
using UnityEngine.Rendering;

namespace Kvant
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("Kvant/Wig Controller")]
    public class WigController : MonoBehaviour
    {
        #region Editable properties

        // Foundation settings

        [SerializeField] Transform _target;

        public Transform target {
            get { return _target; }
            set { _target = value; }
        }

        [SerializeField] WigTemplate _template;

        public WigTemplate template {
            get { return _template; }
            set {
                if (_template != value) {
                    _template = value;
                    _reconfigured = true;
                }
            }
        }

        // Constants

        [SerializeField] float _maxTimeStep = 0.006f;

        public float maxTimeStep {
            get { return _maxTimeStep; }
            set { _maxTimeStep = value; }
        }

        [SerializeField] float _randomSeed;

        public float randomSeed {
            get { return _randomSeed; }
            set {
                if (_randomSeed != value) {
                    _randomSeed = value;
                    _needsReset = true;
                }
            }
        }

        // Filament length

        [SerializeField, Range(0.01f, 5)] float _length = 1;

        public float length {
            get { return _length; }
            set { _length = value; }
        }

        [SerializeField, Range(0, 1)] float _lengthRandomness = 0.5f;

        public float lengthRandomness {
            get { return _lengthRandomness; }
            set { _lengthRandomness = value; }
        }

        // Dynamics

        [SerializeField] float _spring = 600;

        public float spring {
            get { return _spring; }
            set { _spring = value; }
        }

        [SerializeField] float _damping = 30;

        public float damping {
            get { return _damping; }
            set { _damping = value; }
        }

        [SerializeField] Vector3 _gravity = new Vector3(0, -8, 2);

        public Vector3 gravity {
            get { return _gravity; }
            set { _gravity = value; }
        }

        // Noise field

        [SerializeField] float _noiseAmplitude = 5;

        public float noiseAmplitude {
            get { return _noiseAmplitude; }
            set { _noiseAmplitude = value; }
        }

        [SerializeField] float _noiseFrequency = 1;

        public float noiseFrequency {
            get { return _noiseFrequency; }
            set { _noiseFrequency = value; }
        }

        [SerializeField] float _noiseSpeed = 0.1f;

        public float noiseSpeed {
            get { return _noiseSpeed; }
            set { _noiseSpeed = value; }
        }

        #endregion

        #region Public functions

        public void ResetSimulation()
        {
            _needsReset = true;
        }

        #if UNITY_EDITOR

        public void RequestReconfigurationFromEditor()
        {
            _reconfigured = true;
        }

        #endif

        #endregion

        #region Private members

        // Just used to have references to the shader asset.
        [SerializeField, HideInInspector] Shader _kernels;

        // Temporary objects for simulation
        Material _material;
        RenderTexture _positionBuffer1;
        RenderTexture _positionBuffer2;
        RenderTexture _velocityBuffer1;
        RenderTexture _velocityBuffer2;
        RenderTexture _basisBuffer1;
        RenderTexture _basisBuffer2;

        // Custom properties applied to the mesh renderer.
        MaterialPropertyBlock _propertyBlock;

        // Previous position/rotation of the target transform.
        Vector3 _targetPosition;
        Quaternion _targetRotation;

        // Reset flags
        bool _reconfigured = true;
        bool _needsReset;

        // Create a buffer for simulation.
        RenderTexture CreateSimulationBuffer()
        {
            var format = RenderTextureFormat.ARGBFloat;
            var width = _template.filamentCount;
            var buffer = new RenderTexture(width, _template.segmentCount, 0, format);
            buffer.hideFlags = HideFlags.HideAndDontSave;
            buffer.filterMode = FilterMode.Point;
            buffer.wrapMode = TextureWrapMode.Clamp;
            return buffer;
        }

        // Try to release a temporary object.
        void ReleaseObject(Object o)
        {
            if (o != null)
                if (Application.isPlaying)
                    Destroy(o);
                else
                    DestroyImmediate(o);
        }

        // Create and initialize internal temporary objects.
        void SetUpTemporaryObjects()
        {
            if (_material == null)
            {
                var shader = Shader.Find("Hidden/Kvant/Wig/Kernels");
                _material = new Material(shader);
                _material.hideFlags = HideFlags.HideAndDontSave;
            }

            _material.SetTexture("_FoundationData", _template.foundation);
            _material.SetFloat("_RandomSeed", _randomSeed);

            if (_positionBuffer1 == null) _positionBuffer1 = CreateSimulationBuffer();
            if (_positionBuffer2 == null) _positionBuffer2 = CreateSimulationBuffer();
            if (_velocityBuffer1 == null) _velocityBuffer1 = CreateSimulationBuffer();
            if (_velocityBuffer2 == null) _velocityBuffer2 = CreateSimulationBuffer();
            if (_basisBuffer1 == null) _basisBuffer1 = CreateSimulationBuffer();
            if (_basisBuffer2 == null) _basisBuffer2 = CreateSimulationBuffer();
        }

        // Release internal temporary objects.
        void ReleaseTemporaryObjects()
        {
            ReleaseObject(_material); _material = null;
            ReleaseObject(_positionBuffer1); _positionBuffer1 = null;
            ReleaseObject(_positionBuffer2); _positionBuffer2 = null;
            ReleaseObject(_velocityBuffer1); _velocityBuffer1 = null;
            ReleaseObject(_velocityBuffer2); _velocityBuffer2 = null;
            ReleaseObject(_basisBuffer1); _basisBuffer1 = null;
            ReleaseObject(_basisBuffer2); _basisBuffer2 = null;
        }

        // Reset the simulation state.
        void ResetSimulationState()
        {
            _targetPosition = _target.position;
            _targetRotation = _target.rotation;

            UpdateSimulationParameters(_targetPosition, _targetRotation, 0);

            // This is needed to give the texel size for the shader.
            _material.SetTexture("_PositionBuffer", _positionBuffer1);

            Graphics.Blit(null, _positionBuffer2, _material, 0);
            Graphics.Blit(null, _velocityBuffer2, _material, 1);
            Graphics.Blit(null, _basisBuffer2, _material, 2);
        }

        // Update the parameters in the simulation kernels.
        void UpdateSimulationParameters(Vector3 pos, Quaternion rot, float dt)
        {
            _material.SetMatrix("_FoundationTransform",
                Matrix4x4.TRS(pos, rot, Vector3.one)
            );

            _material.SetFloat("_DeltaTime", dt);

            _material.SetVector("_SegmentLength", new Vector2(
                _length / _template.segmentCount, _lengthRandomness
            ));

            _material.SetFloat("_Spring", _spring);
            _material.SetFloat("_Damping", _damping);
            _material.SetVector("_Gravity", _gravity);

            _material.SetVector("_NoiseParams", new Vector3(
                _noiseAmplitude, _noiseFrequency, _noiseSpeed
            ));
        }

        // Invoke the simulation kernels.
        void InvokeSimulationKernels(Vector3 pos, Quaternion rot, float dt)
        {
            // Swap the buffers.
            var pb = _positionBuffer1;
            var vb = _velocityBuffer1;
            var nb = _basisBuffer1;
            _positionBuffer1 = _positionBuffer2;
            _velocityBuffer1 = _velocityBuffer2;
            _basisBuffer1 = _basisBuffer2;
            _positionBuffer2 = pb;
            _velocityBuffer2 = vb;
            _basisBuffer2 = nb;

            // Update the velocity buffer.
            UpdateSimulationParameters(pos, rot, dt);
            _material.SetTexture("_PositionBuffer", _positionBuffer1);
            _material.SetTexture("_VelocityBuffer", _velocityBuffer1);
            Graphics.Blit(null, _velocityBuffer2, _material, 4);

            // Update the position buffer.
            _material.SetTexture("_VelocityBuffer", _velocityBuffer2);
            Graphics.Blit(null, _positionBuffer2, _material, 3);

            // Update the basis buffer.
            _material.SetTexture("_PositionBuffer", _positionBuffer2);
            _material.SetTexture("_BasisBuffer", _basisBuffer1);
            Graphics.Blit(null, _basisBuffer2, _material, 5);
        }

        // Do simulation.
        int Simulate(float deltaTime)
        {
            var newTargetPosition = _target.position;
            var newTargetRotation = _target.rotation;

            var steps = Mathf.CeilToInt(deltaTime / _maxTimeStep);
            steps = Mathf.Clamp(steps, 1, 100);

            var dt = deltaTime / steps;

            for (var i = 0; i < steps; i++)
            {
                var p = (float)i / steps;
                var pos = Vector3.Lerp(_targetPosition, newTargetPosition, p);
                var rot = Quaternion.Lerp(_targetRotation, newTargetRotation, p);
                InvokeSimulationKernels(pos, rot, dt);
            }

            _targetPosition = newTargetPosition;
            _targetRotation = newTargetRotation;

            return steps;
        }

        // Update external components: mesh filter.
        void UpdateMeshFilter()
        {
            var meshFilter = GetComponent<MeshFilter>();

            // Add a new mesh filter if missing.
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
                meshFilter.hideFlags = HideFlags.NotEditable;
            }

            if (meshFilter.sharedMesh != _template.mesh)
                meshFilter.sharedMesh = _template.mesh;
        }

        // Update external components: mesh renderer.
        void UpdateMeshRenderer(float motionScale)
        {
            var meshRenderer = GetComponent<MeshRenderer>();

            if (_propertyBlock == null)
                _propertyBlock = new MaterialPropertyBlock();

            _propertyBlock.SetTexture("_PositionBuffer", _positionBuffer2);
            _propertyBlock.SetTexture("_PreviousPositionBuffer", _positionBuffer1);

            _propertyBlock.SetTexture("_BasisBuffer", _basisBuffer2);
            _propertyBlock.SetTexture("_PreviousBasisBuffer", _basisBuffer1);

            _propertyBlock.SetFloat("_RandomSeed", _randomSeed);
            _propertyBlock.SetFloat("_MotionScale", motionScale);

            meshRenderer.SetPropertyBlock(_propertyBlock);
        }

        #endregion

        #region MonoBehaviour functions

        void Reset()
        {
            _reconfigured = true;
        }

        void OnDestroy()
        {
            ReleaseTemporaryObjects();
        }

        void LateUpdate()
        {
            var motionScale = 1.0f;

            // Do nothing if something is missing.
            if (_template == null || _target == null) return;

            // - Initialize temporary objects at the first frame.
            // - Re-initialize temporary objects on configuration changes.
            if (_reconfigured)
            {
                ReleaseTemporaryObjects();
                SetUpTemporaryObjects();
            }

            // Reset simulation state when it's requested.
            // This also happens when configuration changed.
            if (_needsReset || _reconfigured)
            {
                ResetSimulationState();

                // Editor: do warmup before the first frame.
                if (!Application.isPlaying) motionScale = Simulate(0.4f);

                _needsReset = _reconfigured = false;
            }

            // Advance simulation time.
            if (Application.isPlaying) motionScale = Simulate(Time.deltaTime);

            // Update external components (mesh filter and renderer).
            UpdateMeshFilter();
            UpdateMeshRenderer(motionScale);
        }

        #endregion
    }
}
