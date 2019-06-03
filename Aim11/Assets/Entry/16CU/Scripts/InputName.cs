//===================================================
// ファイル名	：GameManager.cs
// 概要			：ゲームのシーン管理
// 作成者		：藤森 悠輝
// 作成日		：2019.01.07
// 
//---------------------------------------------------
// 更新履歴：
// 2019/01/07 [藤森 悠輝] スクリプト作成
//
//===================================================
using UnityEngine;
using UnityEngine.UI;
using System;

public class InputName : MonoBehaviour
{
    //変数宣言-------------------------------------------------------------------------------------------
    public Text inputText;                                  //入力フィールド
    private string inputString = "";                        //入力用文字列
    private string initText = "";                           //初期文字列
    [SerializeField] private const int textLengthMax = 9;   //文字列の最大数

    public bool compInputName;  //入力完了
    public bool canInput;       //入力許可
    //---------------------------------------------------------------------------------------------------

    /**
     * @fn void Start()
     * @brief　 初期化
     * @param	無し
     * @return	無し
     */
    void Start ()
    {
        //入力フィールドの初期化
        initText = inputText.text;
        //----------------------------------------------------
        //デバッグ用（後で削除）入力関係の判定をtrueに変更
        compInputName = true;
        canInput = true;
        //----------------------------------------------------
    }
    //----------------------------------------------------
    //デバッグ用（後で削除）名前入力の確認
    private void Update()
    {
        InputUserName();
    }
    //----------------------------------------------------

    /**
     * @fn void InputName()
     * @brief 　名前の入力
     * @param	無し
     * @return	無し
     */
    public void InputUserName()
    {
        if (Input.anyKeyDown && canInput)
        {
            //入力されたキーを特定(code)、それによって処理を変更
            foreach(KeyCode code in Enum.GetValues(typeof(KeyCode)))
            {
                if(Input.GetKeyDown(code))
                {
                    switch(code)
                    {
                        //末尾一文字を削除
                        case KeyCode.Backspace:
                            if(inputString.Length > 0)
                            {
                                inputString = inputString.Remove(inputString.Length - 1, 1);
                                inputText.text = inputString;
                            }
                            break;
                        default:
                            //入力がアルファベットなら追加
                            if(IsInputAnAlphabet(code) && inputString.Length < textLengthMax)
                            {
                                inputString += code.ToString();
                                inputText.text = inputString;
                            }
                            break;
                    }
                }
            }
        }
        //テキストに文字がないなら初期文字列に戻す
        if(inputString.Length > 0)
        {
            inputText.text = inputString;
        }
        else
        {
            inputText.text = initText;
        }
    }

    /**
     * @fn void IsInputAnAlphabet()
     * @brief 　アルファベットかどうかを判定
     * @param	無し
     * @return	無し
     */
     private bool IsInputAnAlphabet(KeyCode code)
    {
        if(inputString.Length < textLengthMax)
        {
            if(KeyCode.A <= code && code <= KeyCode.Z)
            {
                return true;
            }
        }
        return false;
    }
}
