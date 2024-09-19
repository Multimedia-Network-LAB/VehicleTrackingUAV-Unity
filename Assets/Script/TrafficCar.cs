using System.Collections;
using UnityEngine;

public class TrafficCar : MonoBehaviour
{
    public float speed = 5.0f;
    public float rotationSpeed = 10.0f;
    public TrfficIntersection TargetTrfficIntersection;
    private TrfficIntersection _currentTrfficIntersection;
    private TrfficIntersection _previousTrfficIntersection;
    private bool isMoving = false;
    //todo ���ͼ��� rightOffset ����ϵ��� ����
    public float rightOffset = 2.0f; // ���������� ġ��ġ�� ����

    private void Start()
    {
        if (TargetTrfficIntersection == null || TargetTrfficIntersection.connectedIntersections.Count == 0)
        {
            Debug.LogError("���� ���� ������ ���� �ʿ�");
            enabled = false;
            return;
        }
        isMoving = true;
    }

    private void Update()
    {
        if (isMoving && TargetTrfficIntersection != null)
        {
            Vector3 direction = (TargetTrfficIntersection.transform.position - transform.position).normalized;
            Vector3 rightOffsetVector = Vector3.Cross(Vector3.up, direction) * rightOffset;
            Vector3 adjustedDirection = (TargetTrfficIntersection.transform.position + rightOffsetVector - transform.position).normalized;
            
            Quaternion targetRotation = Quaternion.LookRotation(adjustedDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            transform.position += adjustedDirection * speed * Time.deltaTime;

            if (Vector3.Distance(transform.position, TargetTrfficIntersection.transform.position) < rightOffset)
            {
                isMoving = false;
                _previousTrfficIntersection = _currentTrfficIntersection;
                _currentTrfficIntersection = TargetTrfficIntersection;
                StartCoroutine(MoveToNextIntersection());
            }
        }
    }

    private IEnumerator MoveToNextIntersection()
    {
        yield return new WaitForSeconds(Random.Range(1.0f, 3.0f)); // ���� ��� �ð�
        do
        {
            TargetTrfficIntersection = _currentTrfficIntersection.connectedIntersections[Random.Range(0, _currentTrfficIntersection.connectedIntersections.Count)];
        } while (TargetTrfficIntersection == _previousTrfficIntersection);
        isMoving = true;
    }
}