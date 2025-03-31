using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UIElements;

namespace Enemy
{
    public class EnemyController : MonoBehaviour
    {
        private Animator _animator;

        private bool _hasAnimator;

        // Start is called before the first frame update
        void Start()
        {
            _hasAnimator = TryGetComponent(out _animator);
        }

        // Update is called once per frame
        void Update()
        {
            Move();
            Rotate();
        }

        private void Rotate()
        {
            throw new NotImplementedException();
        }

        private void Move()
        {
            throw new NotImplementedException();
        }
    }
}

