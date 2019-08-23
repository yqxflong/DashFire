using System;
using UnityEngine;
using System.Collections;

public class GuideTrigger : MonoBehaviour
{
    public GameObject GuideStep2 = null;
    public GameObject GuideStep3 = null;
    public GameObject GuideStep4 = null;
    public GameObject GuideStepEnd = null;

    public void OnLogicTrigger()
    {
        TriggerImpl();
    }

    private void TriggerImpl()
    {
        if (m_GuideStep == 1)
        {

        }
        else if (m_GuideStep == 2)
        {
            if (GuideStep2 != null)
            {
                GuideStep2.SendMessage("OnLogicTrigger");
            }
        }
        else if (m_GuideStep == 3)
        {
            if (GuideStep3 != null)
            {
                GuideStep3.SendMessage("OnLogicTrigger");
            }
        }
        else if (m_GuideStep == 4)
        {
            if (GuideStep4 != null)
            {
                GuideStep4.SendMessage("OnLogicTrigger");
            }
        }
        else if (m_GuideStep == 5)
        {
            if (GuideStepEnd != null)
            {
                GuideStepEnd.SendMessage("OnLogicTrigger");
            }
        }
        m_GuideStep++;
    }

    private int m_GuideStep = 2;
}
