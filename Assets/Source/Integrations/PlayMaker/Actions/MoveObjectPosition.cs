// Thanks to A3DStudio

using System;
using UnityEngine;
using System.Collections.Generic;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Transform)]
	[Tooltip("Move a GameObject to a position using easing functions. Similar to MoveObject but specifying a position rather than a gameobject")]
    public class MoveObjectPosition : EaseFsmAction
    {
        [RequiredField]
        [Tooltip("The GameObject to move.")]
        public FsmOwnerDefault objectToMove;

        [RequiredField]
        [Tooltip("The target destination vector.")]
        public FsmVector3 destination;
        
        private FsmVector3 fromValue;
        private FsmVector3 toVector;
        private FsmVector3 fromVector;

		private bool finishInNextStep;

        public override void Reset()
        {
            base.Reset();
            fromValue = null;
            toVector = null;
            finishInNextStep = false;
            fromVector = null;
        }


        public override void OnEnter()
        {
            base.OnEnter();

            var go = Fsm.GetOwnerDefaultTarget(objectToMove);
            fromVector = go.transform.position;
            toVector = destination;

            fromFloats = new float[3];
            fromFloats[0] = fromVector.Value.x;
            fromFloats[1] = fromVector.Value.y;
            fromFloats[2] = fromVector.Value.z;

            toFloats = new float[3];
            toFloats[0] = toVector.Value.x;
            toFloats[1] = toVector.Value.y;
            toFloats[2] = toVector.Value.z;
            resultFloats = new float[3];

            resultFloats[0] = fromVector.Value.x;
            resultFloats[1] = fromVector.Value.y;
            resultFloats[2] = fromVector.Value.z;

            finishInNextStep = false;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            var go = Fsm.GetOwnerDefaultTarget(objectToMove);
            go.transform.position = new Vector3(resultFloats[0], resultFloats[1], resultFloats[2]);

            if (finishInNextStep)
            {
                Finish();
                if (finishEvent != null) Fsm.Event(finishEvent);
            }

            if (finishAction && !finishInNextStep)
            {
                go.transform.position = new Vector3(reverse.IsNone ? toVector.Value.x : reverse.Value ? fromValue.Value.x : toVector.Value.x,
                reverse.IsNone ? toVector.Value.y : reverse.Value ? fromValue.Value.y : toVector.Value.y,
                reverse.IsNone ? toVector.Value.z : reverse.Value ? fromValue.Value.z : toVector.Value.z
                );
                finishInNextStep = true;
            }
        }
    }
}