using UnityEngine;
using UnityEngine.UI;

public class ExperimentCanvas : MonoBehaviour {

    public Text generationText;
    public Text averageText;
    public Text bestText;

    public void GenerationNumber(int num) {
        generationText.text = "Generation: " + num.ToString();
    }
    public void AverageFitnessNumber(float num) {
        averageText.text = "Previous average: " + num.ToString();
    }
    public void BestFitnessNumber(float num) {
        bestText.text = "Previous best: " + num.ToString();
    }
}
