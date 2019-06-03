using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogitechUpdate : MonoBehaviour
{
	Logitech_test test;
    // Start is called before the first frame update
    void Start()
    {
		test = GetComponent<Logitech_test>();
    }

    // Update is called once per frame
    void Update()
    {
		test.carInputUpdate();
    }
}
