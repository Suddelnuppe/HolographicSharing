using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace HolographicSharing
{
    public class PointerFocus : MonoBehaviour, IMixedRealityFocusHandler, IMixedRealityPointerHandler
    {
        private GameObject tooltip;
        private void Start()
        {
            tooltip = transform.GetChild(0).gameObject;
            tooltip.SetActive(false);
        }
        
        void IMixedRealityFocusHandler.OnFocusEnter(FocusEventData eventData)
        {
            if (eventData.Pointer.InputSourceParent.SourceType == InputSourceType.Hand)
            {
                tooltip.SetActive(true);
            }
        }

        void IMixedRealityFocusHandler.OnFocusExit(FocusEventData eventData)
        {
            if (eventData.Pointer.InputSourceParent.SourceType == InputSourceType.Hand)
            {
                tooltip.SetActive(false);
            }
        }
        
        void IMixedRealityPointerHandler.OnPointerDown(
            MixedRealityPointerEventData eventData) { }

        void IMixedRealityPointerHandler.OnPointerDragged(
            MixedRealityPointerEventData eventData) { }

        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
            throw new System.NotImplementedException();
        }

        void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData)
        {
        }
    }
}