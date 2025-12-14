using System.Collections.Generic;
using System;

[System.Serializable] // Bu satýr Unity'nin bu veriyi kaydetmesini saðlar.
public class WordProgress
{
    public string wordName; // Örn: "apple"
    public int bestScore;   // Örn: 85
    public bool isLearned;  // Örn: true
}

[System.Serializable]
public class SaveFile
{
    // Öðrenilen tüm kelimelerin listesi burada tutulacak
    public List<WordProgress> learnedWords = new List<WordProgress>();
}