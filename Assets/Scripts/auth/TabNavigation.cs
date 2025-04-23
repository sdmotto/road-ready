using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class TabNavigation : MonoBehaviour
{
    void Update()
    {
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

            bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

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
