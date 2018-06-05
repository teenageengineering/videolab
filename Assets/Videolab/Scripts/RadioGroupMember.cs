using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Videolab
{
    public class RadioGroupMember : MonoBehaviour
    {
        #region public

        public int groupIndex = 0;

        #endregion

        delegate void RadioGroupDelegate(RadioGroupMember member);
        static event RadioGroupDelegate OnMemberEnabled;

        int _prevGroupIndex;

        void Awake()
        {
            _prevGroupIndex = groupIndex;
                
            OnMemberEnabled += onMemberEnabled;
        }

        void OnDestroy()
        {
            OnMemberEnabled -= onMemberEnabled;
        }

        void OnEnable()
        {
            OnMemberEnabled(this);
        }

        void Update()
        {
            if (groupIndex != _prevGroupIndex) 
            {
                OnMemberEnabled(this);

                _prevGroupIndex = groupIndex;
            }
        }

        void onMemberEnabled(RadioGroupMember member)
        {
            if (member != this && member.groupIndex == this.groupIndex)
                gameObject.SetActive(false);
        }
    }
}