using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonumentQuestions", menuName = "Historical Explorer/Monument Questions")]
public class MonumentQuestionSet : ScriptableObject
{
    public string monumentName;
    public List<QuestionData> questions;
    public Sprite backgroundImage;
    public Sprite neutralCharacter;
    public Sprite happyCharacter;
    public Sprite angryCharacter;
}