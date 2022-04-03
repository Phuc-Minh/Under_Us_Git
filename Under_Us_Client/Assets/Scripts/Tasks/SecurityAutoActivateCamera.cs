using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityAutoActivateCamera : MonoBehaviour
{
    [MessageHandler((ushort)ServerToClientId.securityCamera)]
    private static void SecurityCamera(Message message)
    {
        bool cameraStatus = message.GetBool();

        GameObject cameraElectrical = GameObject.Find("ElectricalLight").transform.GetChild(0).gameObject;
        GameObject cameraStorage = GameObject.Find("StorageLight").transform.GetChild(0).gameObject;
        GameObject cameraSpeciment = GameObject.Find("SpecimentLight").transform.GetChild(0).gameObject;
        GameObject cameraCommunication = GameObject.Find("CommunicationLight").transform.GetChild(0).gameObject;
        GameObject cameraO2 = GameObject.Find("O2Light").transform.GetChild(0).gameObject;


        cameraElectrical.gameObject.SetActive(cameraStatus);
        cameraStorage.gameObject.SetActive(cameraStatus);
        cameraSpeciment.gameObject.SetActive(cameraStatus);
        cameraCommunication.gameObject.SetActive(cameraStatus);
        cameraO2.gameObject.SetActive(cameraStatus);
    }
}
