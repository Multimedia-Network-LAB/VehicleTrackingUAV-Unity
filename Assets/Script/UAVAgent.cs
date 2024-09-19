using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.UI;

public class UAVAgent: Agent
{
    private Transform tr;
    private Rigidbody rb;
    private RayPerceptionSensorComponent3D raySensorComponent;
    private GameObject detectorCameraObject;
    private Camera detectorCamera;
    private RenderTexture targetTexture;
    private Texture2D carTexture; // �������� �̹��� �ؽ�ó ����
    private Text TextInfo;

    private RaycastHit hitobsFront;

    private Vector3 leftDiagonal = Quaternion.Euler(0, -5, 0) * Vector3.forward;
    private Vector3 rightDiagonal = Quaternion.Euler(0, 5, 0) * Vector3.forward;

    private float reward = 0.0f;

    private Vector3 startPositionWorld; //world ��ǥ�� ���� ��
    private Vector3 endPositionWorld; //�� ��ġ
    private Vector3 hitPosition; //�浹 ��ġ

    private Vector3 startPosition; // ���� ��ġ  (����)
    private Vector3 startRotation; // ���� ����    (����)

    private const float disObs = 30.0f; //��ֹ��� �����ϱ� ���� ����ĳ��Ʈ�� �ִ� �Ÿ�.
    private static float horizontalSpeed = 6.0f; // ���� �ӵ�
    private static float verticalSpeed = 3.0f; // ���� �ӵ�

    private Vector3 horizontalDirection; // ���� ����
    private Vector3 verticalDirection; // ���� ����
    private Vector3 finalDirection; // ���� ���� ���� ��

    private readonly Vector3 m_Forward = Vector3.forward * horizontalSpeed;
    private readonly Vector3 m_Back = Vector3.back * horizontalSpeed;
    private readonly Vector3 m_Left = Vector3.left * horizontalSpeed;
    private readonly Vector3 m_Right = Vector3.right * horizontalSpeed;
    private readonly Vector3 m_ForwardLeft = (Vector3.forward + Vector3.left).normalized * horizontalSpeed;
    private readonly Vector3 m_ForwardRight = (Vector3.forward + Vector3.right).normalized * horizontalSpeed;
    private readonly Vector3 m_BackLeft = (Vector3.back + Vector3.left).normalized * horizontalSpeed;
    private readonly Vector3 m_BackRight = (Vector3.back + Vector3.right).normalized * horizontalSpeed;

    private readonly Vector3 m_Up = Vector3.up * verticalSpeed;
    private readonly Vector3 m_Down = Vector3.down * verticalSpeed;

    private float disLeft = 0f;
    private float disRight = 0f;
    private Color[] pixels; //�ȼ� �����͸� ��Ÿ���µ� ���,.

    private CarMover carMover;
    private Vector3 carPostion;
    private float distance = 0.0f;

    //todo ������ ���� �迭

    //todo text info ����
    private float totalReward = 0.0f;
    private int episode = -1;
    private int step = 0;
    private bool wasVehicleDetected = false;  // 이전 스텝에서 차량이 감지되었는지 여부


    public override void Initialize() // �ʱ�ȭ �޼ҵ�
    {
        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        raySensorComponent = GetComponent<RayPerceptionSensorComponent3D>();

        startPosition = tr.position;
        startRotation = tr.eulerAngles;

        detectorCameraObject = GameObject.Find("DetectCamera");

        detectorCamera = detectorCameraObject.GetComponent<Camera>();
        detectorCamera.targetTexture = new RenderTexture(16, 16, 16);
        targetTexture = detectorCamera.targetTexture;
        
        carMover = FindObjectOfType<CarMover>();
        TextInfo = GameObject.Find("TextInfo").GetComponent<Text>();
        TextInfo.text = "Start///";
    }

    public override void OnEpisodeBegin() // ���Ǽҵ� ���۸��� ȣ��
    {
        // �ӵ��� ���ӵ� �ʱ�ȭ
        //rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // ��ġ �ʱ�ȭ 
        tr.position = startPosition;
        tr.eulerAngles = startRotation;
        carTexture = new Texture2D(targetTexture.width, targetTexture.height, TextureFormat.RGB565, false);
        SetReward(0);

        horizontalDirection = Vector3.zero;
        verticalDirection = Vector3.zero;
        step = 0;
        episode++;
    }

    private bool detectVehicle()
    {
        targetTexture = detectorCamera.targetTexture;
        RenderTexture.active = targetTexture;
        detectorCamera.Render();

        carTexture.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);
        carTexture.Apply();
        pixels = carTexture.GetPixels();

        foreach (Color pixel in pixels)
        {
            if (pixel.r > 0.01f || pixel.g > 0.01f || pixel.b > 0.01f)
            {
                carPostion = carMover.CurrentPosition;
                // �������� �ƴ� �ȼ��� �ϳ��� �߰ߵǸ� true�� ��ȯ
                return true;
            }
        }

        return false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var action = actions.DiscreteActions[0];
        reward = 0.0f;
        SetReward(0);
        step++;
        //action = 0;
        //todo action
        switch (action)
        {
            case 0: // ���� ����
                horizontalDirection = Vector3.zero;
                break;
            case 1: // ����
                horizontalDirection = m_Forward;
                break;
            case 2: // ����
                horizontalDirection = m_Back;
                break;
            case 3: // ��
                horizontalDirection = m_Left;
                break;
            case 4: // ��
                horizontalDirection = m_Right;
                break;
            case 5: // ������ �밢
                horizontalDirection = m_ForwardLeft;
                break;
            case 6: // ������ �밢
                horizontalDirection = m_ForwardRight;
                break;
            case 7: // ������ �밢
                horizontalDirection = m_BackLeft;
                break;
            case 8: // ������ �밢
                horizontalDirection = m_BackRight;
                break;
            // case 9: // ���� ����
            //     verticalDirection = Vector3.zero;
            //     break;
            // case 10: // ���� ���
            //     verticalDirection = m_Up;
            //     break;
            // case 11: // ���� �ϰ�
            //     verticalDirection = m_Down;
            //     break;
            default: // ���� ó��
                horizontalDirection = Vector3.zero;
                verticalDirection = Vector3.zero;
                break;
        }
        
        // 차량 포착에 따른 reward
        bool isVehicleDetected = detectVehicle();

        if (isVehicleDetected && wasVehicleDetected)
        {
            reward += 1.0f;  // 차량이 계속 포착되고 있는 경우
        }
        else if (isVehicleDetected && !wasVehicleDetected)
        {
            reward += 10.0f;  // 차량이 새롭게 포착된 경우
        }
        else if (!isVehicleDetected && wasVehicleDetected)
        {
            reward += -10.0f;  // 차량이 사라진 경우
        }
        else
        {
            reward += -1.0f;  // 차량이 계속 포착되지 않는 경우
        }

        wasVehicleDetected = isVehicleDetected;  // 상태 업데이트

        // finalDirection = horizontalDirection + verticalDirection;
        transform.Translate(horizontalDirection * Time.deltaTime);

        SetReward(reward);
        totalReward = GetCumulativeReward();

        if (detectVehicle())
        {
            distance = Vector3.Distance(carPostion, tr.position);
        }

        //todo text info
        var info = $"Episode: {episode}\n" +
                   $"Step: {step}\n" +
                   $"Action: {action}\n" +
                   //$"Speed: {moveSpeed}\n" +
                   $"Detect : {isVehicleDetected}\n" +
                   $"Reward: {reward}\n" +
                   $"EpiReward: {totalReward}\n" +
                   "";
        TextInfo.text = info;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actionOut = actionsOut.DiscreteActions;
        //actionOut[0] = 0;
        if (Input.GetKey(KeyCode.W)) actionOut[0] = 1;
        if (Input.GetKey(KeyCode.S)) actionOut[0] = 2;
        if (Input.GetKey(KeyCode.A)) actionOut[0] = 3;
        if (Input.GetKey(KeyCode.D)) actionOut[0] = 4;
        if (Input.GetKey(KeyCode.Q)) actionOut[0] = 0;
        if (Input.GetKey(KeyCode.R)) actionOut[0] = 10;
        if (Input.GetKey(KeyCode.V)) actionOut[0] = 11;
        if (Input.GetKey(KeyCode.F)) actionOut[0] = 9;
    }
}