using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace bibCorp
{
    public class UI_ConfirmationDialogue : MonoBehaviour
    {
        [SerializeField] Text m_Message = default;
        [SerializeField] Button m_Btn_Yes = default;
        [SerializeField] Button m_Btn_No = default;

        public void Set(string pText, UnityAction pCallback_Yes)
        {
            m_Message.text = pText;
            m_Btn_Yes.onClick.AddListener(pCallback_Yes);
            m_Btn_Yes.onClick.AddListener(Close);
            m_Btn_No.onClick.AddListener(Close);
        }

        private void Close()
        {
            Destroy(this.gameObject);
        }
    }
}
