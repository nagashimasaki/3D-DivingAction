﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private PlayerController player;     // キャラの位置情報

    [SerializeField]
    private Transform goal = null;       // ゴール地点(水面)の位置情報

    //距離の値を受け取って更新するためのコンポーネントを代入する
    [SerializeField]
    private Text txtDistance;

    [SerializeField]
    private CameraController cameraController;

    [SerializeField]
    private ResultPopUp resultPopUp;

    [SerializeField]
    private AudioManager audioManager;


    [SerializeField, Header("ステージをランダム生成する場合にはチェックする")]
    private bool isRandomStaging;

    [SerializeField, Header("移動する花輪の割合"), Range(0, 100)]
    private int movingFlowerCirclePercent;

    [SerializeField, Header("大きさが変化する花輪の割合"), Range(0, 100)]
    private int scalingFlowerCirclePercent;

    [SerializeField]
    private FlowerCircle flowerCirclePrefab;　　　　// 花輪のプレファブ・ゲームオブジェクトにアタッチされている FlowerCircle スクリプトをアサインする(同じプレファブ)

    [SerializeField]
    private Transform limitLeftBottom;　　　　　　　// キャラの移動制限用のオブジェクトを生成位置の制限にも利用する

    [SerializeField]
    private Transform limitRightTop;　　　　　　　　// キャラの移動制限用のオブジェクトを生成位置の制限にも利用する


    // キャラと水面までの距離の計測用
    private float distance;

    // ゴール判定用。距離が 0 以下になったらゴールと判定して true にする。false の間はゴールしていない状態(着水判定と同じ bool 型の利用方法)
    private bool isGoal;                

    void Update()
    {
        // 距離が 0 以下になったらゴールしたと判定して距離の計算は行わないようにする
        if (isGoal == true)
        {
            // return があると、この処理よりも下の処理は処理されない
            return;
        }

        // Y軸が高さの情報なので、双方の高さの値を減算して差分値を距離とする
        distance = player.transform.position.y - goal.position.y;

        // Consoleビューに距離を表示する
        //Debug.Log(distance.ToString("F2"));
        txtDistance.text = distance.ToString("F2");

        // 距離が 0 以下になったら
        if (distance <= 0)
        {
            // 距離が 0 以下になったので、ゴールと判定する
            isGoal = true;

            // 距離を 0 にする
            distance = 0;

            // カメラを初期のカメラに戻す
            cameraController.SetDefaultCamera();

            // リザルト表示
            resultPopUp.DisplayResult();

            // ゲームクリアのBGMを再生する
            audioManager.PlayBGM(AudioManager.BgmType.GameClear);
        }
    }


    IEnumerator Start()　　　　　　　　　// 戻り値が void ではないので注意
    {
        // Updateを止める
        isGoal = true;

        // 花輪をランダムで配置する場合
        if (isRandomStaging)
        {

            // 花輪の生成処理を行う。この処理が終了するまで、次の処理を中断する
            yield return StartCoroutine(CreateRandomStage());
        }

        // Updateを再開
        isGoal = false;
        Debug.Log(isGoal);
    }

    /// <summary>
    /// ランダムで花輪を生成してステージ作成
    /// </summary>
    private IEnumerator CreateRandomStage()
    {

        // 花輪の高さのスタート位置
        float flowerHeight = goal.position.y;

        // 花輪を生成した数
        int count = 0;
        Debug.Log("初期の花輪のスタート位置 : " + flowerHeight);

        // 花輪の高さがキャラの位置に到達するまで、ループ処理を行って花輪を生成する。キャラの位置に到達したらループを終了する
        while (flowerHeight <= player.transform.position.y)
        {

            // 花輪の高さを加算(float 型の Random.Range メソッドは 10.0f を含む)
            flowerHeight += Random.Range(5.0f, 10.0f);

            Debug.Log("現在の花輪の生成位置 : " + flowerHeight);

            // 花輪の位置を設定して生成
            FlowerCircle flowerCircle = Instantiate(flowerCirclePrefab, new Vector3(Random.Range(limitLeftBottom.position.x, limitRightTop.position.x), flowerHeight, Random.Range(limitLeftBottom.position.z, limitRightTop.position.z)), Quaternion.identity);

            // 花輪の初期設定を呼び出す。引数には評価後の戻り値を利用する。このとき、移動するかどうか、大きさを変えるかどうかの情報を引数として渡す
            flowerCircle.SetUpMovingFlowerCircle(Random.Range(0, 100) <= movingFlowerCirclePercent, Random.Range(0, 100) <= scalingFlowerCirclePercent);

            // 花輪の生成数を加算
            count++;

            Debug.Log("花輪の合計生成数 : " + count);

            // 1フレームだけ中断。　　※　この処理を入れないと無限ループしてUnityがフリーズします。
            yield return null;
        }

        Debug.Log("ランダムステージ完成");
    }

}
