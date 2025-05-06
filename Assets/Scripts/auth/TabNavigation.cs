using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// this script allows the user to tab (or shift-tab) between entries on the login/signup page
public class TabNavigation : MonoBehaviour
{
    void Update()
    {
        // check for tab press
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (current == null)
            {
                return;
            }

            Selectable currentSelectable = current.GetComponent<Selectable>();
            if (currentSelectable == null)
            {
                return;
            }
            // bool for if shift key is held
            bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            // move up or down depening on if shift is held 
            Selectable next = shiftHeld
                ? currentSelectable.FindSelectableOnUp()
                : currentSelectable.FindSelectableOnDown();

            if (next == null)
            {
                return;
            }

            EventSystem.current.SetSelectedGameObject(next.gameObject);

            TMP_InputField tmp = next.GetComponent<TMP_InputField>();
            if (tmp != null)
            {
                tmp.OnPointerClick(new PointerEventData(EventSystem.current));
            }
        }
    }
}
