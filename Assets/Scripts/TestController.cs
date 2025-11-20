using UnityEngine;

namespace VoiceCommand
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Animator))]
    public class TestController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("Movement speed in m/s")]
        public float MoveSpeed = 2.5f;

        [Tooltip("Rotation speed when turning")]
        public float RotationSpeed = 10f;

        [Tooltip("Distance threshold to consider arrived at target")]
        public float ArrivalThreshold = 0.3f;

        [Tooltip("Distance to move forward before dancing when sitting")]
        public float DanceForwardDistance = 1.0f;

        [Header("Position References")]
        [Tooltip("Left position target")]
        public Transform LeftPosition;

        [Tooltip("Right position target")]
        public Transform RightPosition;

        [Tooltip("Chair transform (use child transform for exact sit position)")]
        public Transform ChairSitPosition;

        // Components
        private Rigidbody _rigidbody;
        private Animator _animator;

        // Animation parameter hashes
        private int _animIDSpeed;
        private int _animIDDance;
        private int _animIDWave;
        private int _animIDSit;

        // State
        private Vector3 _targetPosition;
        private bool _isMovingToTarget;
        private bool _isSitting;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();

            // Configure Rigidbody
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            // Cache animation parameter hashes
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDDance = Animator.StringToHash("Dance");
            _animIDWave = Animator.StringToHash("Wave");
            _animIDSit = Animator.StringToHash("Sit");
        }

        private void FixedUpdate()
        {
            if (_isMovingToTarget)
            {
                MoveToTarget();
            }
        }

        #region Public Command Methods

        /// <summary>
        /// Move to left position
        /// </summary>
        public void MoveLeft()
        {
            if (LeftPosition == null)
            {
                Debug.LogWarning("TestController: LeftPosition is not assigned!");
                return;
            }

            if (_isSitting)
            {
                StandUp();
            }

            StartMovingToPosition(LeftPosition.position);
        }

        /// <summary>
        /// Move to right position
        /// </summary>
        public void MoveRight()
        {
            if (RightPosition == null)
            {
                Debug.LogWarning("TestController: RightPosition is not assigned!");
                return;
            }

            if (_isSitting)
            {
                StandUp();
            }

            StartMovingToPosition(RightPosition.position);
        }

        /// <summary>
        /// Play dance animation
        /// </summary>
        public void Dance()
        {
            if (_isSitting)
            {
                // Move forward a bit and stand up before dancing
                StandUp(PlayDanceAnimation);
            }
            else
            {
                PlayDanceAnimation();
            }
        }

        /// <summary>
        /// Play wave (greeting) animation
        /// </summary>
        public void Wave()
        {
            if (_isSitting)
            {
                StandUp(PlayWaveAnimation);
            }
            else
            {
                PlayWaveAnimation();
            }
        }

        /// <summary>
        /// Sit on chair
        /// </summary>
        public void SitOnChair()
        {
            if (ChairSitPosition == null)
            {
                Debug.LogWarning("TestController: ChairSitPosition is not assigned!");
                return;
            }

            if (_isSitting)
            {
                Debug.Log("TestController: Already sitting");
                return;
            }

            // Move to chair position and sit
            StartMovingToPosition(ChairSitPosition.position, () => SitDown());
        }

        #endregion

        #region Movement Logic

        private void StartMovingToPosition(Vector3 targetPos, System.Action onArrival = null)
        {
            _targetPosition = targetPos;
            _isMovingToTarget = true;
            _onArrivalCallback = onArrival;
        }

        private System.Action _onArrivalCallback;

        private void MoveToTarget()
        {
            Vector3 currentPos = transform.position;
            Vector3 direction = (_targetPosition - currentPos);
            direction.y = 0; // Ignore vertical difference
            float distance = direction.magnitude;

            // Check if arrived
            if (distance <= ArrivalThreshold)
            {
                _isMovingToTarget = false;
                _animator.SetFloat(_animIDSpeed, 0f);

                // Invoke callback if exists
                _onArrivalCallback?.Invoke();
                _onArrivalCallback = null;

                return;
            }

            // Move towards target
            direction.Normalize();

            // Rotate towards movement direction using Rigidbody.MoveRotation
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                Quaternion newRotation = Quaternion.Slerp(_rigidbody.rotation, targetRotation, RotationSpeed * Time.fixedDeltaTime);
                _rigidbody.MoveRotation(newRotation);
            }

            // Move character using Rigidbody.MovePosition
            Vector3 newPosition = _rigidbody.position + direction * MoveSpeed * Time.fixedDeltaTime;
            _rigidbody.MovePosition(newPosition);

            // Update animator
            _animator.SetFloat(_animIDSpeed, MoveSpeed);
        }

        #endregion

        #region Animation Actions

        private void PlayDanceAnimation()
        {
            _animator.SetTrigger(_animIDDance);
        }

        private void PlayWaveAnimation()
        {
            _animator.SetTrigger(_animIDWave);
        }

        private void SitDown()
        {
            _isSitting = true;
            _animator.SetBool(_animIDSit, true);

            // Match chair position and rotation using Rigidbody
            if (ChairSitPosition != null)
            {
                _rigidbody.MovePosition(ChairSitPosition.position);
                _rigidbody.MoveRotation(ChairSitPosition.rotation);
            }
        }

        private void StandUp(System.Action onArrival = null)
        {
            _isSitting = false;
            _animator.SetBool(_animIDSit, false);

            Vector3 forwardPos = transform.position + transform.forward * DanceForwardDistance;
            StartMovingToPosition(forwardPos, () => onArrival?.Invoke());
        }

        #endregion

        #region Debug Visualization

        private void OnDrawGizmosSelected()
        {
            // Draw position references
            if (LeftPosition != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(LeftPosition.position, 0.3f);
                Gizmos.DrawLine(transform.position, LeftPosition.position);
            }

            if (RightPosition != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(RightPosition.position, 0.3f);
                Gizmos.DrawLine(transform.position, RightPosition.position);
            }

            if (ChairSitPosition != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(ChairSitPosition.position, 0.3f);
                Gizmos.DrawLine(transform.position, ChairSitPosition.position);

                // Draw chair forward direction
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(ChairSitPosition.position, ChairSitPosition.forward * 0.5f);
            }

            // Draw arrival threshold
            if (_isMovingToTarget)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_targetPosition, ArrivalThreshold);
            }
        }

        #endregion
    }
}
