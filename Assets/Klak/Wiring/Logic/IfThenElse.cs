using UnityEngine;

namespace Klak.Wiring
{
    [AddComponentMenu("Klak/Wiring/Logic/IfThenElse")]
    public class IfThenElse : NodeBase
    {
       	#region Editable properties

        [SerializeField]
        bool _continuousInput;

        [SerializeField]
        Condition _condition1;

        [SerializeField]
        bool _useGate;

        [SerializeField]
        Gate _gate;

        [SerializeField]
        Condition _condition2;

        #endregion

        #region Node I/O

        [Inlet]
        public float input {
            set {
                if (!enabled) return;

                _inputValue = value;

                if (!_continuousInput) _currentState = State.Dormant;

                _satisfy1 = CheckCondition(_inputValue, _compareValue1, _condition1);

                if (!_useGate)
                    Invoke(_satisfy1);

                else
                {
                    _satisfy2 = CheckCondition(_inputValue, _compareValue2, _condition2);

                    if (_gate == Gate.AND)
                        Invoke(_satisfy1 && _satisfy2);

                    if (_gate == Gate.OR)
                        Invoke(_satisfy1 || _satisfy2);
                }
            }
        }

        [Inlet]
        public float compareValue1 {
            set{
                _compareValue1 = value;
            }
        }

        [Inlet]
        public float compareValue2 {
            set {
                _compareValue2 = value;
            }
        }

        [SerializeField, Outlet]
        VoidEvent _thenEvent = new VoidEvent();

        [SerializeField, Outlet]
        VoidEvent _elseEvent = new VoidEvent();

        #endregion

        #region Private members

        enum Gate { OR, AND }
        enum State { Dormant, BlockThenEvent, BlockElseEvent }
        State _currentState;
        enum Condition { Greater, Less, Equal }
        float _inputValue;
        float _compareValue1;
        float _compareValue2;
        bool _satisfy1;
        bool _satisfy2;

        private bool CheckCondition(float input, float comparevalue, Condition condition)
        {
            switch (condition)
            {
                case Condition.Less:

                    return input < comparevalue;

                case Condition.Greater:

                    return input > comparevalue;

                case Condition.Equal:

                    return input == comparevalue;
            }
            return false;
        }

        private void Invoke(bool then)
        {
            if (then)
            {
                if (_currentState != State.BlockThenEvent)
                {
                    _thenEvent.Invoke();
                    if (_continuousInput) _currentState = State.BlockThenEvent;
                }
            }

            else
            {
                if (_currentState != State.BlockElseEvent)
                {
                    _elseEvent.Invoke();
                    if (_continuousInput) _currentState = State.BlockElseEvent;
                }
            }
        }

        #endregion
    }
}
