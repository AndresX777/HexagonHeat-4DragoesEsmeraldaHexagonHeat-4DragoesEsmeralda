using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartController : MonoBehaviour
{
    public GameObject countdownCanvas;

    public GameObject num3;
    public GameObject num2;
    public GameObject num1;
    public GameObject inicio;

    public string sceneToLoad = "Escenario";

    public void StartCountdown()
    {
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        countdownCanvas.SetActive(true);

        // 3
        num3.SetActive(true);
        yield return new WaitForSeconds(1f);
        num3.SetActive(false);

        // 2
        num2.SetActive(true);
        yield return new WaitForSeconds(1f);
        num2.SetActive(false);

        // 1
        num1.SetActive(true);
        yield return new WaitForSeconds(1f);
        num1.SetActive(false);

        // Start
        inicio.SetActive(true);
        yield return new WaitForSeconds(1f);
        inicio.SetActive(false);

        SceneManager.LoadScene(sceneToLoad);
    }
}
