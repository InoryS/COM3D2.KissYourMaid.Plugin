using System;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using UnityInjector.Attributes;
using PluginExt;
using System.Collections.Generic;

// コンパイル用コマンド （ ..\COM3D2\ 部分は環境に合わせて変更）
// C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /o /t:library /lib:"..\COM3D2\Sybaris" /lib:"..\COM3D2\Sybaris\UnityInjector" /lib:"..\COM3D2\COM3D2x64_Data\Managed" /r:UnityEngine.dll /r:UnityEngine.VR.dll /r:UnityInjector.dll /r:Assembly-CSharp.dll /r:Assembly-CSharp-firstpass.dll /r:COM3D2.ExternalSaveData.Managed.dll /r:PluginExt.dll COM3D2.KissYourMaid.Plugin.cs

[assembly: AssemblyTitle("KissYourMaid")]
[assembly: AssemblyVersion("0.2.1.2")]

namespace COM3D2.KissYourMaid.Plugin
{
    [
        PluginFilter("COM3D2x64"), PluginFilter("COM3D2VRx64"), PluginFilter("COM3D2OHx64"),
        PluginFilter("COM3D2OHVRx64"),
        PluginName("KissYourMaid"), PluginVersion("0.2.1.2")
    ]
    public class KissYourMaid : ExPluginBase
    {
        //　設定クラス（Iniファイルで読み書きしたい変数はここに格納する）
        class KissYourMaidConfig
        {
            //　キー設定
            public KeyCode keyPluginToggle = KeyCode.H; //　本プラグインの有効無効の切替キー（手動操作にも使う）
            public KeyCode keyManualForceAlt = KeyCode.Mouse2; //　（切替キーとの同時押し）パターンを変える
            public KeyCode keyManualIncrease = KeyCode.Mouse1; //　（手替キーとの同時押し）激しめにする
            public KeyCode keyManualDecrease = KeyCode.Mouse0; //　（切替キーとの同時押し）おとなしめにする

            //　一般設定

            public bool bPluginEnabled = true; //　本プラグインの有効状態（夜伽）
            public bool bPluginEnabledInEdit = true; //　本プラグインの有効状態（エディット）
            public bool bVoiceOverrideEnabled = true; //　キス時の音声オーバライド（上書き）機能を使う
            public bool bDontLookAtMeInFellatio = false; //　フェラ時に目の追従を行わせない
            public int iYodareAppearLevel = 0; //　所定の興奮レベル以上でよだれをつける（１～４のどれかを入れる、０で無効）
            public int iExciteLevelThreshold1 = 150; //　興奮レベル１→２閾値
            public int iExciteLevelThreshold2 = 200; //　興奮レベル２→３閾値
            public int iExciteLevelThreshold3 = 250; //　興奮レベル３→４閾値

            public bool bPluginValidADV = true; //  シーンID=15 (SceenADV)をすべて有効にする
            public bool bPluginValidEvent = true; //  回想、訓練イベント、ライフモード等の会話系のシーンで有効にする
            public bool bPluginValidPrivate = true; //  ホーム、プライベート、嫁イドイベントで有効にする
            public bool bPluginValidGuest = true; //  ゲストモードで有効にする

            public int iFpsMax = 60; //最大Fps 処理間隔や表情変更の秒数に影響
            public int iFpsMaxVR = 90; //VRのFps 処理間隔や表情変更の秒数に影響

            //　不機嫌モード・顔目そらし設定
            public bool bFukigenEnabled = false; //　不機嫌モードの有効状態
            public bool bOnRestoreLookAtMe = true; //　離れた時にこちらを見る様にするか
            public int iPercentLookAway = 0; //　顔目そらし確率
            public int iPercentLookAwayFukigen = 100; //　顔目そらし確率（不機嫌モード）
            public int iPercentLookAwayOtherParts = 50; //　顔目そらし確率（口以外の部位へのキス時）
            public bool bLookAwayOnlyEye = false; //　顔目そらしの際、目だけそらすか
            public bool bLookAwayOnlyEyeFukigen = false; //　顔目そらしの際、目だけそらすか（不機嫌モード）
            public bool bLookAwayOnlyEyeOtherParts = false; //　顔目そらしの際、目だけそらすか（口以外の部位へのキス時）

            //　口以外へのキス
            public bool bTargetOtherParts = true; //　口以外の部位へのキスを有効にする
            public bool bTargetMune = true; //　胸へのキスを有効にする
            public bool bTargetKokan = true; //　股間へのキスを有効にする
            public bool bTargetFoots = false; //　足へのキスを有効にする（デフォルトでは無効、使わない人には邪魔だと思うので）

            //　痙攣設定
            public bool bConvulsionEnabled = true; //　（口以外の部位へのキス時の）痙攣を有効にする
            public int iConvulsionChancePercentExcite1 = 20; //  興奮度１の時の痙攣確率（0.5秒毎にチェック）
            public int iConvulsionChancePercentExcite2 = 30; //  興奮度２の時の痙攣確率（0.5秒毎にチェック）
            public int iConvulsionChancePercentExcite3 = 40; //  興奮度３の時の痙攣確率（0.5秒毎にチェック）
            public int iConvulsionChancePercentExcite4 = 50; //  興奮度４の時の痙攣確率（0.5秒毎にチェック）
            public int iConvulsionTimeFramesBase = 7; //　痙攣時間（フレーム数）
            public int iConvulsionTimeFramesBaseRandomExtend = 3; //　痙攣時間へのランダム加算
            public float fConvulsionSpeedMultiplierBase = 7.0f; //　痙攣でのアニメーション速度倍率
            public float fConvulsionSpeedMultiplierRandomExtend = 3.0f; //　痙攣でのアニメーション速度倍率へのランダム加算


            //　距離判定
            public float fDistanceThresholdBase1 = 0.60f; //　「近い」の判定距離
            public float fDistanceThresholdBase2 = 0.40f; //　「とても近い」の距離距離
            public float fDistanceThresholdOffset = 0.20f; //　 帰り方向で判定距離をずらす距離
            public float fDistanceThresholdBase1VR = 0.45f; //　(VR時)「近い」の判定距離
            public float fDistanceThresholdBase2VR = 0.30f; //　(VR時)「とても近い」の距離距離
            public float fDistanceThresholdOffsetVR = 0.10f; //　(VR時)帰り方向で判定距離をずらす距離

            public float fCheckOffsetHead = -0.01f; // 口と胸の近いほうの判定距離オフセット マイナスで口が近く判定される
            public float fDistanceOffsetMune = -0.05f; // 胸の「とても近い」判定距離オフセット マイナスで距離が短くなる
            public float fDistanceOffsetKokan = -0.05f; // 股間の「とても近い」判定距離オフセット マイナスで距離が短くなる
            public float fDistanceOffsetFoot = 0f; // 足の「とても近い」判定距離オフセット マイナスで距離が短くなる

            //　表情管理
            public int iStateAltTime1Base = 2; //　フェイスアニメの変化時間１（秒）（20→21の遷移、40→41の遷移）
            public int iStateAltTime2Base = 6; //　フェイスアニメの変化時間２（秒）（30におけるランダム再遷移）
            public int iStateAltTime1RandomExtend = 2; //　変化時間１へのランダム加算（秒）
            public int iStateAltTime2RandomExtend = 6; //　変化時間２へのランダム加算（秒）
            public float fAnimeFadeTime = 3.0f; //　フェイスアニメ等のフェード時間（秒）

            //　表情テーブル　（全性格共通）
            public string[] sFaceAnime20 = new string[] { "変更しない" };
            public string[] sFaceAnime21 = new string[] { "目口閉じ" };
            public string[] sFaceAnime40 = new string[] { "困った" };
            public string[] sFaceAnime41 = new string[] { "優しさ", "微笑み" };

            public string[][] sFaceAnime30Excite = new string[][]
            {
                //口へのキス用
                new string[] { "接吻", "エロ羞恥２", "ダンスキス", "ダンス憂い", "エロ舐め通常", "閉じ舐め通常" },
                new string[] { "閉じフェラ通常", "エロ好感３", "エロ羞恥３", "エロ舐め快楽", "閉じフェラ愛情", "閉じ舐め愛情" },
                new string[] { "まぶたギュ", "エロ舌責", "エロ痛み３", "エロ舐め愛情", "エロ舐め嫌悪", "閉じ舐め快楽" },
                new string[] { "エロ痛み我慢３", "エロ興奮３", "エロ舌責快楽", "閉じフェラ嫌悪", "閉じ舐め嫌悪", "エロ痛み２" }
            };

            public string[][] sFaceAnime30ExciteMune = new string[][]
            {
                //口以外へのキス用
                new string[] { "困った", "ダンス困り顔", "恥ずかしい", "苦笑い", "ためいき", "まぶたギュ" },
                new string[] { "怒り", "興奮射精後１", "発情", "エロ痛み２", "あーん", "エロ我慢３" },
                new string[] { "エロ痛み１", "エロ痛み２", "エロ我慢１", "エロ我慢２", "泣き", "怒り" },
                new string[] { "エロ痛み我慢", "エロ痛み我慢２", "エロ痛み我慢３", "エロメソ泣き", "エロ羞恥３", "エロ我慢３" },
            };

            //　表情テーブル　（全性格共通）　不機嫌モード
            public string[] sFaceAnime20Fukigen = new string[] { "恥ずかしい", "照れ", "拗ね" };
            public string[] sFaceAnime21Fukigen = new string[] { "ぷんすか" };
            public string[] sFaceAnime40Fukigen = new string[] { "困った", "泣き", "ダンス憂い" };
            public string[] sFaceAnime41Fukigen = new string[] { "恥ずかしい", "怒り", "拗ね", "ダンスジト目" };

            public string[][] sFaceAnime30FukigenExcite = new string[][]
            {
                //口へのキス用
                new string[] { "怒り", "エロ羞恥２", "エロ我慢２", "エロ痛み我慢", "エロフェラ嫌悪", "閉じ舐め嫌悪" },
                new string[] { "閉じフェラ嫌悪", "エロ我慢３", "エロ羞恥３", "エロ痛み我慢２", "エロ舐め嫌悪", "少し怒り" },
                new string[] { "まぶたギュ", "エロ舌責", "エロ痛み３", "エロ舐め愛情", "エロ舐め嫌悪", "エロフェラ嫌悪" },
                new string[] { "エロ痛み我慢３", "エロ興奮３", "エロ舌責快楽", "閉じフェラ嫌悪", "閉じ舐め嫌悪", "エロ痛み２" }
            };

            public string[][] sFaceAnime30FukigenExciteMune = new string[][]
            {
                //口以外へのキス用
                new string[] { "困った", "ダンス困り顔", "恥ずかしい", "苦笑い", "ためいき", "まぶたギュ" },
                new string[] { "怒り", "興奮射精後１", "発情", "エロ痛み２", "あーん", "エロ我慢３" },
                new string[] { "エロ痛み１", "エロ痛み２", "エロ我慢１", "エロ我慢２", "泣き", "怒り" },
                new string[] { "エロ痛み我慢", "エロ痛み我慢２", "エロ痛み我慢３", "エロメソ泣き", "エロ羞恥３", "エロ我慢３" },
            };

            //　性格別声テーブル　通常版（興奮度別）
            //　キス抽送 *_rr_0003b.ks　と　フェラ抽送 *_rr_00084a.ks に記載の音声を振り分けた

            // ツンデレ
            public string[][] sLoopVoice30PrideExcite = new string[][]
            {
                new string[] { "s0_01276.ogg", "s0_01277.ogg", "s0_01278.ogg", "s0_01279.ogg" },
                new string[] { "s0_01284.ogg", "s0_01285.ogg", "s0_01286.ogg", "s0_01287.ogg" },
                new string[] { "s0_01280.ogg", "s0_01281.ogg", "s0_01282.ogg", "s0_01283.ogg" },
                new string[] { "s0_01288.ogg", "s0_01289.ogg", "s0_01290.ogg", "s0_01291.ogg" },
            };

            // クーデレ
            public string[][] sLoopVoice30CoolExcite = new string[][]
            {
                new string[] { "s1_02349.ogg", "s1_02350.ogg", "s1_02351.ogg", "s1_02352.ogg" },
                new string[] { "s1_02357.ogg", "s1_02358.ogg", "s1_02359.ogg", "s1_02360.ogg" },
                new string[] { "s1_02353.ogg", "s1_02354.ogg", "s1_02355.ogg", "s1_02356.ogg" },
                new string[] { "s1_02361.ogg", "s1_02362.ogg", "s1_02363.ogg", "s1_02364.ogg" },
            };

            // 純真
            public string[][] sLoopVoice30PureExcite = new string[][]
            {
                new string[] { "s2_01190.ogg", "s2_01191.ogg", "s2_01192.ogg", "s2_01193.ogg" },
                new string[] { "s2_01198.ogg", "s2_01199.ogg", "s2_01200.ogg", "s2_01201.ogg" },
                new string[] { "s2_01194.ogg", "s2_01195.ogg", "s2_01196.ogg", "s2_01197.ogg" },
                new string[] { "s2_01202.ogg", "s2_01203.ogg", "s2_01204.ogg", "s2_01205.ogg" },
            };

            // ヤンデレ
            public string[][] sLoopVoice30YandereExcite = new string[][]
            {
                new string[]
                    { "s3_12044.ogg", "s3_02728.ogg", "s3_02729.ogg", "s3_02730.ogg" }, //s3_02727.ogg → s3_12044.ogg
                new string[] { "s3_02735.ogg", "s3_02736.ogg", "s3_02737.ogg", "s3_02738.ogg" },
                new string[] { "s3_02731.ogg", "s3_02732.ogg", "s3_02733.ogg", "s3_02734.ogg" },
                new string[] { "s3_02739.ogg", "s3_02740.ogg", "s3_02741.ogg", "s3_02742.ogg" },
            };

            // お姉ちゃん
            public string[][] sLoopVoice30AnesanExcite = new string[][]
            {
                new string[] { "s4_08167.ogg", "s4_08168.ogg", "s4_08169.ogg", "s4_08170.ogg" },
                new string[] { "s4_08175.ogg", "s4_08176.ogg", "s4_08177.ogg", "s4_08178.ogg" },
                new string[] { "s4_08171.ogg", "s4_08172.ogg", "s4_08173.ogg", "s4_08174.ogg" },
                new string[] { "s4_08179.ogg", "s4_08180.ogg", "s4_08181.ogg", "s4_08182.ogg" },
            };

            // ボクッ娘
            public string[][] sLoopVoice30GenkiExcite = new string[][]
            {
                new string[] { "s5_04087.ogg", "s5_04088.ogg", "s5_04089.ogg", "s5_04090.ogg" },
                new string[] { "s5_04095.ogg", "s5_04096.ogg", "s5_04097.ogg", "s5_04098.ogg" },
                new string[] { "s5_04091.ogg", "s5_04092.ogg", "s5_04093.ogg", "s5_04094.ogg" },
                new string[] { "s5_04099.ogg", "s5_04100.ogg", "s5_04101.ogg", "s5_04102.ogg" },
            };

            // ドＳ
            public string[][] sLoopVoice30SadistExcite = new string[][]
            {
                // g_rr_0003b (2219-2226) & g_rr_00084a (2227-2234)
                new string[]
                    { "s6_02219.ogg", "s6_02220.ogg", "s6_02221.ogg", "s6_02222.ogg" }, // g_rr_0003b.ks  の前半４つ
                new string[] { "s6_02227.ogg", "s6_02228.ogg", "s6_02229.ogg", "s6_02230.ogg" }, // g_rr_00084a.ks の前半４つ
                new string[] { "s6_02223.ogg", "s6_02224.ogg", "s6_02225.ogg", "s6_02226.ogg" }, // g_rr_0003b.ks  の後半４つ
                new string[] { "s6_02231.ogg", "s6_02232.ogg", "s6_02233.ogg", "s6_02234.ogg" }, // g_rr_00084a.ks の後半４つ
            };

            // 無垢
            public string[][] sLoopVoice30MukuExcite = new string[][]
            {
                // g_rr_0003b (2219-2226) & g_rr_00084a (2227-2234)
                new string[] { "H0_00093.ogg", "H0_00094.ogg" }, //ca003aキス(L0-L3)
                new string[] { "H0_00095.ogg", "H0_00096.ogg" },
                new string[] { "H0_00097.ogg", "H0_00253.ogg" }, //ca003aキス(L4) + ca003k発情フェラ(L0-L2)
                new string[] { "H0_00254.ogg", "H0_00255.ogg" },
            };

            // 真面目
            public string[][] sLoopVoice30MajimeExcite = new string[][]
            {
                // g_rr_0003b (2219-2226) & g_rr_00084a (2227-2234)
                new string[] { "H1_00265.ogg", "H1_00266.ogg" },
                new string[] { "H1_00267.ogg", "H1_00268.ogg" },
                new string[] { "H1_00269.ogg", "H1_00270.ogg" },
                new string[] { "H1_00271.ogg", "H1_00272.ogg" },
            };

            // 凜デレ
            public string[][] sLoopVoice30RindereExcite = new string[][]
            {
                // g_rr_0003b (2219-2226) & g_rr_00084a (2227-2234)
                new string[] { "H2_00067.ogg", "H2_00068.ogg" },
                new string[] { "H2_00069.ogg", "H2_00070.ogg" },
                new string[] { "H2_00071.ogg", "H2_00072.ogg" },
                new string[] { "H2_00073.ogg", "H2_00074.ogg" },
            };

            // 文学少女
            public string[][] sLoopVoice30SilentExcite = new string[][]
            {
                new string[] { "H3_00566.ogg", "H3_00567.ogg" },
                new string[] { "H3_00568.ogg", "H3_00569.ogg" },
                new string[] { "H3_00570.ogg", "H3_00571.ogg" },
                new string[] { "H3_00572.ogg", "H3_00573.ogg" },
            };

            // 小悪魔
            public string[][] sLoopVoice30DevilishExcite = new string[][]
            {
                new string[] { "H4_00901.ogg", "H4_00902.ogg" },
                new string[] { "H4_00903.ogg", "H4_00904.ogg" },
                new string[] { "H4_00905.ogg", "H4_00906.ogg" },
                new string[] { "H4_00907.ogg", "H4_00908.ogg" },
            };

            // おしとやか
            public string[][] sLoopVoice30LadylikeExcite = new string[][]
            {
                new string[] { "H5_00640.ogg", "H5_00641.ogg" },
                new string[] { "H5_00642.ogg", "H5_00643.ogg" },
                new string[] { "H5_00644.ogg", "H5_00645.ogg" },
                new string[] { "H5_00646.ogg", "H5_00647.ogg" },
            };

            // メイド秘書
            public string[][] sLoopVoice30SecretaryExcite = new string[][]
            {
                new string[] { "H6_00206.ogg", "H6_00207.ogg" },
                new string[] { "H6_00208.ogg", "H6_00209.ogg" },
                new string[] { "H6_00210.ogg", "H6_00211.ogg" },
                new string[] { "H6_00212.ogg", "H6_00213.ogg" },
            };

            // 妹
            public string[][] sLoopVoice30SisterExcite = new string[][]
            {
                new string[] { "H7_02810.ogg", "H7_02811.ogg" },
                new string[] { "H7_02812.ogg", "H7_02813.ogg" },
                new string[] { "H7_02814.ogg", "H7_02815.ogg" },
                new string[] { "H7_02816.ogg", "H7_02817.ogg" },
            };

            // 無愛想
            public string[][] sLoopVoice30CurtnessExcite = new string[][]
            {
                new string[] { "H8_01179.ogg", "H8_01180.ogg" },
                new string[] { "H8_01181.ogg", "H8_01182.ogg" },
                new string[] { "H8_01183.ogg", "H8_01184.ogg" },
                new string[] { "H8_01185.ogg", "H8_01186.ogg" },
            };

            // お嬢様
            public string[][] sLoopVoice30MissyExcite = new string[][]
            {
                new string[] { "H9_00618.ogg", "H9_00619.ogg" },
                new string[] { "H9_00620.ogg", "H9_00621.ogg" },
                new string[] { "H9_00622.ogg", "H9_00623.ogg" },
                new string[] { "H9_00624.ogg", "H9_00625.ogg" },
            };

            // 幼馴染
            public string[][] sLoopVoice30ChildhoodExcite = new string[][]
            {
                new string[] { "H10_03889.ogg", "H10_03890.ogg" },
                new string[] { "H10_03891.ogg", "H10_03892.ogg" },
                new string[] { "H10_03893.ogg", "H10_03894.ogg" },
                new string[] { "H10_03895.ogg", "H10_03896.ogg" },
            };

            // ドＭ
            public string[][] sLoopVoice30MasochistExcite = new string[][]
            {
                new string[] { "H11_00713.ogg", "H11_00714.ogg" }, //ca003aキス(L0～L7)
                new string[] { "H11_00715.ogg", "H11_00716.ogg" },
                new string[] { "H11_00717.ogg", "H11_00718.ogg" },
                new string[] { "H11_00719.ogg", "H11_00720.ogg" },
            };

            // 腹黒
            public string[][] sLoopVoice30CraftyExcite = new string[][]
            {
                new string[] { "H12_01253.ogg", "H12_01254.ogg" }, //ca003aキス(L0～L7)
                new string[] { "H12_01255.ogg", "H12_01256.ogg" },
                new string[] { "H12_01257.ogg", "H12_01258.ogg" },
                new string[] { "H12_01259.ogg", "H12_01260.ogg" },
            };

            // 気さく
            public string[][] sLoopVoice30FriendlyExcite = new string[][]
            {
                new string[] { "V1_00530.ogg", "V1_00531.ogg" }, //ca003aキス(L0～L7)
                new string[] { "V1_00532.ogg", "V1_00533.ogg" },
                new string[] { "V1_00534.ogg", "V1_00535.ogg" },
                new string[] { "V1_00536.ogg", "V1_00537.ogg" },
            };

            // 淑女
            public string[][] sLoopVoice30DameExcite = new string[][]
            {
                new string[] { "V0_00528.ogg", "V0_00529.ogg" }, //ca003aキス(L0～L7)
                new string[] { "V0_00530.ogg", "V0_00531.ogg" },
                new string[] { "V0_00532.ogg", "V0_00533.ogg" },
                new string[] { "V0_00534.ogg", "V0_00535.ogg" },
            };

            //ギャル
            public string[][] sLoopVoice30GalExcite = new string[][]
            {
                new string[] { "H13_01084.ogg", "H13_01085.ogg" }, //ca003aキス(L0～L7)
                new string[] { "H13_01086.ogg", "H13_01087.ogg" },
                new string[] { "H13_01088.ogg", "H13_01089.ogg" },
                new string[] { "H13_01090.ogg", "H13_01091.ogg" },
            };

            //　性格別声テーブル　キス以外の汎用（興奮度別）
            //  基本的には、「メイドがポーズを取っている時の吐息ボイス」*_rr_00010a ＋　「メイドがパイズリなど奉仕をする時の吐息ボイス」*_rr_00049a.ks　で構成
            // （※ツンデレだけは前者が控えめの声だったので「アナルセックス等でメイドが漏らす苦しさのある喘ぎのボイス」を使用）
            //　各スクリプトに８個の音声があったが、そのうち２つは苦悶で合わない様に感じたので除外した
            //　そのため興奮度レベル１と２のテーブルは、ChuBLipのテーブルのように使い回しに

            // ツンデレ
            public string[][] sLoopVoice30PrideMune = new string[][]
            {
                new string[] { "S0_01316.ogg", "S0_01317.ogg" },
                new string[] { "S0_01318.ogg", "S0_01319.ogg" },
                new string[] { "S0_103950.ogg", "S0_103951.ogg" },
                new string[] { "S0_103952.ogg", "S0_103953.ogg" },
            };

            // クーデレ
            public string[][] sLoopVoice30CoolMune = new string[][]
            {
                new string[] { "S1_02389.ogg", "S1_02390.ogg" },
                new string[] { "S1_02391.ogg", "S1_02392.ogg" },
                new string[] { "S1_102717.ogg", "S1_102718.ogg" },
                new string[] { "S1_102719.ogg", "S1_102720.ogg" },
            };

            // 純真
            public string[][] sLoopVoice30PureMune = new string[][]
            {
                new string[] { "S2_01234.ogg", "s2_01256.ogg" }, //ca002a待機(L4) + ca001i奉仕(L2)
                new string[] { "S2_01235.ogg", "s2_01257.ogg" }, //ca002a待機(L5) + ca001i奉仕(L3)
                new string[] { "S2_01236.ogg", "s2_01258.ogg" }, //ca002a待機(L6) + ca001i奉仕(L4)
                new string[] { "S2_01237.ogg", "s2_01259.ogg" }, //ca002a待機(L7) + ca001i奉仕(L5)
            };

            // ヤンデレ
            public string[][] sLoopVoice30YandereMune = new string[][]
            {
                new string[] { "S3_02767.ogg", "S3_02768.ogg" },
                new string[] { "S3_02769.ogg", "S3_02770.ogg" },
                new string[] { "S3_101125.ogg", "S3_101126.ogg" },
                new string[] { "S3_101127.ogg", "S3_101128.ogg" },
            };

            // お姉ちゃん
            public string[][] sLoopVoice30AnesanMune = new string[][]
            {
                new string[] { "S4_08207.ogg", "S4_08208.ogg" },
                new string[] { "S4_08209.ogg", "S4_08210.ogg" },
                new string[] { "S4_102930.ogg", "S4_102931.ogg" },
                new string[] { "S4_102932.ogg", "S4_102933.ogg" },
            };

            // ボクッ娘
            public string[][] sLoopVoice30GenkiMune = new string[][]
            {
                new string[] { "S5_04127.ogg", "S5_04128.ogg" },
                new string[] { "S5_04129.ogg", "S5_04130.ogg" },
                new string[] { "S5_102337.ogg", "S5_102338.ogg" },
                new string[] { "S5_102339.ogg", "S5_102340.ogg" },
            };

            // ドＳ
            public string[][] sLoopVoice30SadistMune = new string[][]
            {
                new string[] { "S6_02259.ogg", "S6_02260.ogg" },
                new string[] { "S6_02261.ogg", "S6_02262.ogg" },
                new string[] { "S6_102978.ogg", "S6_10297.ogg" },
                new string[] { "S6_102980.ogg", "S6_102981.ogg" },
            };

            // 無垢
            public string[][] sLoopVoice30MukuMune = new string[][]
            {
                new string[] { "H0_00237.ogg", "H0_00238.ogg", "H0_00135.ogg" },
                new string[] { "H0_00239.ogg", "H0_00240.ogg", "H0_00136.ogg" },
                new string[] { "H0_00229.ogg", "H0_00230.ogg" },
                new string[] { "H0_00231.ogg", "H0_00232.ogg" },
            };

            // 真面目
            public string[][] sLoopVoice30MajimeMune = new string[][]
            {
                new string[] { "H1_00409.ogg", "H1_00410.ogg" },
                new string[] { "H1_00411.ogg", "H1_00412.ogg" },
                new string[] { "H1_00401.ogg", "H1_00402.ogg" },
                new string[] { "H1_00403.ogg", "H1_00404.ogg" },
            };

            // 凜デレ
            public string[][] sLoopVoice30RindereMune = new string[][]
            {
                new string[] { "H2_00211.ogg", "H2_00212.ogg" },
                new string[] { "H2_00213.ogg", "H2_00214.ogg" },
                new string[] { "H2_00203.ogg", "H2_00204.ogg" },
                new string[] { "H2_00205.ogg", "H2_00206.ogg" },
            };

            // 文学少女
            public string[][] sLoopVoice30SilentMune = new string[][]
            {
                new string[] { "H3_00742.ogg", "H3_00743.ogg" },
                new string[] { "H3_00744.ogg", "H3_00745.ogg" },
                new string[] { "H3_00734.ogg", "H3_00735.ogg" },
                new string[] { "H3_00736.ogg", "H3_00737.ogg" },
            };

            // 小悪魔
            public string[][] sLoopVoice30DevilishMune = new string[][]
            {
                new string[] { "H4_01077.ogg", "H4_01078.ogg" },
                new string[] { "H4_01079.ogg", "H4_01080.ogg" },
                new string[] { "H4_01069.ogg", "H4_01070.ogg" },
                new string[] { "H4_01071.ogg", "H4_01072.ogg" },
            };

            // おしとやか
            public string[][] sLoopVoice30LadylikeMune = new string[][]
            {
                new string[] { "H5_00816.ogg", "H5_00817.ogg" },
                new string[] { "H5_00818.ogg", "H5_00819.ogg" },
                new string[] { "H5_00808.ogg", "H5_00809.ogg" },
                new string[] { "H5_00810.ogg", "H5_00811.ogg" },
            };

            // メイド秘書
            public string[][] sLoopVoice30SecretaryMune = new string[][]
            {
                new string[] { "H6_00382.ogg", "H6_00383.ogg" },
                new string[] { "H6_00384.ogg", "H6_00385.ogg" },
                new string[] { "H6_00374.ogg", "H6_00375.ogg" },
                new string[] { "H6_00376.ogg", "H6_00377.ogg" },
            };

            // 妹
            public string[][] sLoopVoice30SisterMune = new string[][]
            {
                new string[] { "H7_02978.ogg", "H7_02989.ogg" },
                new string[] { "H7_02979.ogg", "H7_02990.ogg" },
                new string[] { "H7_02980.ogg", "H7_02991.ogg" },
                new string[] { "H7_02981.ogg", "H7_02992.ogg" },
            };

            // 無愛想
            public string[][] sLoopVoice30CurtnessMune = new string[][]
            {
                new string[] { "H8_01355.ogg", "H8_01356.ogg" },
                new string[] { "H8_01357.ogg", "H8_01358.ogg" },
                new string[] { "H8_01347.ogg", "H8_01348.ogg" },
                new string[] { "H8_01349.ogg", "H8_01350.ogg" },
            };

            // お嬢様
            public string[][] sLoopVoice30MissyMune = new string[][]
            {
                new string[] { "H9_00794.ogg", "H9_00795.ogg" },
                new string[] { "H9_00796.ogg", "H9_00797.ogg" },
                new string[] { "H9_00786.ogg", "H9_00787.ogg" },
                new string[] { "H9_00788.ogg", "H9_00789.ogg" },
            };

            // 幼馴染
            public string[][] sLoopVoice30ChildhoodMune = new string[][]
            {
                new string[] { "H10_04065.ogg", "H10_04066.ogg" },
                new string[] { "H10_04067.ogg", "H10_04068.ogg" },
                new string[] { "H10_04057.ogg", "H10_04058.ogg" },
                new string[] { "H10_04059.ogg", "H10_04060.ogg" },
            };

            // ドＭ
            public string[][] sLoopVoice30MasochistMune = new string[][]
            {
                new string[] { "H11_00761.ogg", "H11_00762.ogg" }, //ca002a待機
                new string[] { "H11_00763.ogg", "H11_00764.ogg" },
                new string[] { "H11_00745.ogg", "H11_00746.ogg" }, //ca003cオナニー
                new string[] { "H11_00747.ogg", "H11_00748.ogg" },
            };

            // 腹黒
            public string[][] sLoopVoice30CraftyMune = new string[][]
            {
                new string[] { "H12_01309.ogg", "H12_01310.ogg" },
                new string[] { "H12_01311.ogg", "H12_01312.ogg" },
                new string[] { "H12_01421.ogg", "H12_01422.ogg" },
                new string[] { "H12_01423.ogg", "H12_01424.ogg" },
            };

            // 気さく
            public string[][] sLoopVoice30FriendlyMune = new string[][]
            {
                new string[] { "V1_00585.ogg", "V1_00589.ogg" }, //未使用音声(L7) + ca002a待機(L3)
                new string[] { "V1_00592.ogg", "V1_00593.ogg" }, //ca002a待機(L6,L7)
                new string[] { "V1_00698.ogg", "V1_00699.ogg" }, //ca001j発情(L0,L1)
                new string[] { "V1_00700.ogg", "V1_00701.ogg" }, //ca001j発情(L2,L3)
            };

            // 淑女
            public string[][] sLoopVoice30DameMune = new string[][]
            {
                new string[] { "V0_00588.ogg", "V0_00589.ogg" }, //ca002a待機(L2,L3)
                new string[] { "V0_00590.ogg", "V0_00591.ogg" }, //ca002a待機(L4,L5)
                new string[] { "V0_00698.ogg", "V0_00699.ogg" }, //ca001j発情(L2,L3)
                new string[] { "V0_00700.ogg", "V0_00701.ogg" }, //ca001j発情(L4,L5)
            };

            // ギャル  ※ca002a待機は股間で使用
            public string[][] sLoopVoice30GalMune = new string[][]
            {
                new string[] { "H13_01237.ogg", "H13_01136.ogg" }, //ca001h初心(L1) + ft_00001処女喪失(L4)
                new string[] { "H13_01238.ogg", "H13_01137.ogg" }, //ca001h初心(L2) + ft_00001処女喪失(L5)
                new string[] { "H13_01239.ogg", "H13_01138.ogg" }, //ca001h初心(L3) + ft_00001処女喪失(L6)
                new string[] { "H13_01240.ogg", "H13_01139.ogg" }, //ca001h初心(L4) + ft_00001処女喪失(L7)
            };

            // 股間のボイス設定
            // 未設定ならExciteMuneの音声が利用される
            // ca001h初心(L0～L5) ca001j発情(L0～L5) あたりの音声を設定
            public string[][] sLoopVoice30PrideKokan = new string[][]
            {
                new string[] { "S0_01308.ogg", "S0_103950.ogg" },
                new string[] { "S0_01309.ogg", "S0_103951.ogg" },
                new string[] { "S0_01310.ogg", "S0_103952.ogg" },
                new string[] { "S0_01311.ogg", "S0_103953.ogg" },
            };

            public string[][] sLoopVoice30CoolKokan = new string[][]
            {
                new string[] { "S1_02381.ogg", "S1_102717.ogg" },
                new string[] { "S1_02382.ogg", "S1_102718.ogg" },
                new string[] { "S1_02383.ogg", "S1_102719.ogg" },
                new string[] { "S1_02384.ogg", "S1_102720.ogg" },
            };

            public string[][] sLoopVoice30PureKokan = new string[][]
            {
                new string[] { "S2_01241.ogg", "s2_01175.ogg" },
                new string[] { "S2_01242.ogg", "s2_01176.ogg" },
                new string[] { "S2_01243.ogg", "s2_01166.ogg" },
                new string[] { "S2_01245.ogg", "s2_01171.ogg" },
            };

            public string[][] sLoopVoice30YandereKokan = new string[][]
            {
                new string[] { "S3_02847.ogg", "S3_101125.ogg" },
                new string[] { "S3_02848.ogg", "S3_101126.ogg" },
                new string[] { "S3_02849.ogg", "S3_101127.ogg" },
                new string[] { "S3_02848.ogg", "S3_101128.ogg" },
            };

            public string[][] sLoopVoice30AnesanKokan = new string[][]
            {
                new string[] { "S4_08199.ogg", "S4_102930.ogg" },
                new string[] { "S4_08200.ogg", "S4_102931.ogg" },
                new string[] { "S4_08201.ogg", "S4_102932.ogg" },
                new string[] { "S4_08202.ogg", "S4_102933.ogg" },
            };

            public string[][] sLoopVoice30GenkiKokan = new string[][]
            {
                new string[] { "S5_04119.ogg", "S5_102337.ogg" },
                new string[] { "S5_04120.ogg", "S5_102338.ogg" },
                new string[] { "S5_04121.ogg", "S5_102339.ogg" },
                new string[] { "S5_04122.ogg", "S5_102340.ogg" },
            };

            public string[][] sLoopVoice30SadistKokan = new string[][]
            {
                new string[] { "S6_02251.ogg", "S6_02179.ogg" },
                new string[] { "S6_02252.ogg", "S6_02180.ogg" },
                new string[] { "S6_02253.ogg", "S6_02180.ogg" },
                new string[] { "S6_02254.ogg", "S6_02180.ogg" },
            };

            public string[][] sLoopVoice30MukuKokan = new string[][]
            {
                new string[] { "H0_00213.ogg", "H0_09226.ogg" },
                new string[] { "H0_00214.ogg", "H0_09227.ogg" },
                new string[] { "H0_00215.ogg", "H0_09228.ogg" },
                new string[] { "H0_00216.ogg", "H0_09229.ogg" },
            };

            public string[][] sLoopVoice30MajimeKokan = new string[][]
            {
                new string[] { "H1_00225.ogg", "H1_08968.ogg" },
                new string[] { "H1_00226.ogg", "H1_08969.ogg" },
                new string[] { "H1_00227.ogg", "H1_08970.ogg" },
                new string[] { "H1_00228.ogg", "H1_08971.ogg" },
            };

            public string[][] sLoopVoice30RindereKokan = new string[][]
            {
                new string[] { "H2_00099.ogg", "H2_09843.ogg" },
                new string[] { "H2_00100.ogg", "H2_09844.ogg" },
                new string[] { "H2_00101.ogg", "H2_09845.ogg" },
                new string[] { "H2_00102.ogg", "H2_09846.ogg" },
            };

            public string[][] sLoopVoice30SilentKokan = new string[][]
            {
                new string[] { "H3_00606.ogg", "H3_00734.ogg" },
                new string[] { "H3_00607.ogg", "H3_00735.ogg" },
                new string[] { "H3_00608.ogg", "H3_00736.ogg" },
                new string[] { "H3_00609.ogg", "H3_00737.ogg" },
            };

            public string[][] sLoopVoice30DevilishKokan = new string[][]
            {
                new string[] { "H4_00941.ogg", "H4_01069.ogg" },
                new string[] { "H4_00942.ogg", "H4_01070.ogg" },
                new string[] { "H4_00943.ogg", "H4_01071.ogg" },
                new string[] { "H4_00944.ogg", "H4_01072.ogg" },
            };

            public string[][] sLoopVoice30LadylikeKokan = new string[][]
            {
                new string[] { "H5_00792.ogg", "H5_00700.ogg" }, //ca001h初心(L0-L3) + 愛撫ft_00003(L4-L7)
                new string[] { "H5_00793.ogg", "H5_00701.ogg" },
                new string[] { "H5_00794.ogg", "H5_00702.ogg" },
                new string[] { "H5_00795.ogg", "H5_00703.ogg" },
            };

            public string[][] sLoopVoice30SecretaryKokan = new string[][]
            {
                new string[] { "H6_00246.ogg", "H6_00374.ogg" },
                new string[] { "H6_00247.ogg", "H6_00375.ogg" },
                new string[] { "H6_00248.ogg", "H6_00376.ogg" },
                new string[] { "H6_00249.ogg", "H6_00377.ogg" },
            };

            public string[][] sLoopVoice30SisterKokan = new string[][]
            {
                new string[] { "H7_02850.ogg", "H7_02997.ogg" },
                new string[] { "H7_02851.ogg", "H7_02998.ogg" },
                new string[] { "H7_02852.ogg", "H7_02999.ogg" },
                new string[] { "H7_02853.ogg", "H7_03000.ogg" },
            };

            public string[][] sLoopVoice30CurtnessKokan = new string[][]
            {
                new string[] { "H8_01219.ogg", "H8_01347.ogg" },
                new string[] { "H8_01220.ogg", "H8_01348.ogg" },
                new string[] { "H8_01221.ogg", "H8_01349.ogg" },
                new string[] { "H8_01222.ogg", "H8_01350.ogg" },
            };

            public string[][] sLoopVoice30MissyKokan = new string[][]
            {
                new string[] { "H9_00770.ogg", "H9_00787.ogg" },
                new string[] { "H9_00771.ogg", "H9_00788.ogg" },
                new string[] { "H9_00772.ogg", "H9_00789.ogg" },
                new string[] { "H9_00773.ogg", "H9_00790.ogg" },
            };

            public string[][] sLoopVoice30ChildhoodKokan = new string[][]
            {
                new string[] { "H10_04041.ogg", "H10_04057.ogg" },
                new string[] { "H10_04042.ogg", "H10_04058.ogg" },
                new string[] { "H10_04043.ogg", "H10_04059.ogg" },
                new string[] { "H10_04044.ogg", "H10_04060.ogg" },
            };

            public string[][] sLoopVoice30MasochistKokan = new string[][]
            {
                new string[] { "H11_00849.ogg", "H11_00865.ogg" },
                new string[] { "H11_00850.ogg", "H11_00866.ogg" },
                new string[] { "H11_00851.ogg", "H11_00867.ogg" },
                new string[] { "H11_00852.ogg", "H11_00868.ogg" },
            };

            public string[][] sLoopVoice30CraftyKokan = new string[][]
            {
                new string[] { "H12_01293.ogg", "H12_01421.ogg" },
                new string[] { "H12_01294.ogg", "H12_01422.ogg" },
                new string[] { "H12_01295.ogg", "H12_01423.ogg" },
                new string[] { "H12_01296.ogg", "H12_01424.ogg" },
            };

            public string[][] sLoopVoice30FriendlyKokan = new string[][]
            {
                new string[] { "V1_00570.ogg", "V1_00698.ogg" },
                new string[] { "V1_00571.ogg", "V1_00699.ogg" },
                new string[] { "V1_00572.ogg", "V1_00670.ogg" },
                new string[] { "V1_00573.ogg", "V1_00671.ogg" },
            };

            public string[][] sLoopVoice30DameKokan = new string[][]
            {
                new string[] { "V0_00568.ogg", "V0_00696.ogg" },
                new string[] { "V0_00569.ogg", "V0_00697.ogg" },
                new string[] { "V0_00570.ogg", "V0_00698.ogg" },
                new string[] { "V0_00571.ogg", "V0_00699.ogg" },
            };

            public string[][] sLoopVoice30GalKokan = new string[][]
            {
                new string[] { "H13_01143.ogg", "H13_01253.ogg" }, //ca002a待機(L3-L5,L7) + ca001j発情(L1,L3-L5)
                new string[] { "H13_01144.ogg", "H13_01255.ogg" },
                new string[] { "H13_01145.ogg", "H13_01256.ogg" },
                new string[] { "H13_01147.ogg", "H13_01257.ogg" },
            };

            //カスタムボイス設定 string[][][] だと読み込んでくれないためLvごとの配列に分けて設定
            public string[] sLoopVoice30CustomMaidName = new string[] { };

            public string[][] sLoopVoice30CustomExciteLv0 = new string[][] { };
            public string[][] sLoopVoice30CustomExciteLv1 = new string[][] { };
            public string[][] sLoopVoice30CustomExciteLv2 = new string[][] { };
            public string[][] sLoopVoice30CustomExciteLv3 = new string[][] { };

            public string[][] sLoopVoice30CustomMuneLv0 = new string[][] { };
            public string[][] sLoopVoice30CustomMuneLv1 = new string[][] { };
            public string[][] sLoopVoice30CustomMuneLv2 = new string[][] { };
            public string[][] sLoopVoice30CustomMuneLv3 = new string[][] { };

            public string[][] sLoopVoice30CustomKokanLv0 = new string[][] { };
            public string[][] sLoopVoice30CustomKokanLv1 = new string[][] { };
            public string[][] sLoopVoice30CustomKokanLv2 = new string[][] { };
            public string[][] sLoopVoice30CustomKokanLv3 = new string[][] { };
        }


        public bool bGlobalPluginEnabled = true;

        //カスタムボイス 内部格納用
        public string[][][] sLoopVoice30CustomExcite = null;
        public string[][][] sLoopVoice30CustomMune = null;

        public string[][][] sLoopVoice30CustomKokan = null;

        //設定ファイルから通常の配列形式に変換して格納
        private void setCustomVoice()
        {
            sLoopVoice30CustomExcite = new string[cfg.sLoopVoice30CustomExciteLv0.Length][][];
            for (int i = 0; i < cfg.sLoopVoice30CustomExciteLv0.Length; i++)
            {
                string[][] voice = new string[4][];
                voice[0] = cfg.sLoopVoice30CustomExciteLv0.Length > i
                    ? cfg.sLoopVoice30CustomExciteLv0[i]
                    : new string[0];
                voice[1] = cfg.sLoopVoice30CustomExciteLv1.Length > i
                    ? cfg.sLoopVoice30CustomExciteLv1[i]
                    : new string[0];
                voice[2] = cfg.sLoopVoice30CustomExciteLv2.Length > i
                    ? cfg.sLoopVoice30CustomExciteLv2[i]
                    : new string[0];
                voice[3] = cfg.sLoopVoice30CustomExciteLv3.Length > i
                    ? cfg.sLoopVoice30CustomExciteLv3[i]
                    : new string[0];
                sLoopVoice30CustomExcite[i] = voice;
            }

            sLoopVoice30CustomMune = new string[cfg.sLoopVoice30CustomMuneLv0.Length][][];
            for (int i = 0; i < cfg.sLoopVoice30CustomMuneLv0.Length; i++)
            {
                string[][] voice = new string[4][];
                voice[0] = cfg.sLoopVoice30CustomMuneLv0.Length > i ? cfg.sLoopVoice30CustomMuneLv0[i] : new string[0];
                voice[1] = cfg.sLoopVoice30CustomMuneLv1.Length > i ? cfg.sLoopVoice30CustomMuneLv1[i] : new string[0];
                voice[2] = cfg.sLoopVoice30CustomMuneLv2.Length > i ? cfg.sLoopVoice30CustomMuneLv2[i] : new string[0];
                voice[3] = cfg.sLoopVoice30CustomMuneLv3.Length > i ? cfg.sLoopVoice30CustomMuneLv3[i] : new string[0];
                sLoopVoice30CustomMune[i] = voice;
            }

            sLoopVoice30CustomKokan = new string[cfg.sLoopVoice30CustomKokanLv0.Length][][];
            for (int i = 0; i < cfg.sLoopVoice30CustomKokanLv0.Length; i++)
            {
                string[][] voice = new string[4][];
                voice[0] = cfg.sLoopVoice30CustomKokanLv0.Length > i
                    ? cfg.sLoopVoice30CustomKokanLv0[i]
                    : new string[0];
                voice[1] = cfg.sLoopVoice30CustomKokanLv1.Length > i
                    ? cfg.sLoopVoice30CustomKokanLv1[i]
                    : new string[0];
                voice[2] = cfg.sLoopVoice30CustomKokanLv2.Length > i
                    ? cfg.sLoopVoice30CustomKokanLv2[i]
                    : new string[0];
                voice[3] = cfg.sLoopVoice30CustomKokanLv3.Length > i
                    ? cfg.sLoopVoice30CustomKokanLv3[i]
                    : new string[0];
                sLoopVoice30CustomKokan[i] = voice;
            }

            //コンソールに表示
            if (cfg.sLoopVoice30CustomMaidName.Length > 0)
            {
                Console.WriteLine("[KissYourMaid] カスタムボイス設定");
                for (int i = 0; i < cfg.sLoopVoice30CustomMaidName.Length; i++)
                {
                    Console.WriteLine("CustomMaidName[" + i + "] \"" + cfg.sLoopVoice30CustomMaidName[i] + "\"");
                    if (i < sLoopVoice30CustomExcite.Length)
                    {
                        for (int j = 0; j < sLoopVoice30CustomExcite[i].Length; j++)
                        {
                            Console.WriteLine(" CustomExcite[" + i + "][" + j + "] " +
                                              string.Join(",", sLoopVoice30CustomExcite[i][j]));
                        }
                    }

                    if (i < sLoopVoice30CustomMune.Length)
                    {
                        for (int j = 0; j < sLoopVoice30CustomMune[i].Length; j++)
                        {
                            Console.WriteLine(" CustomMune  [" + i + "][" + j + "] " +
                                              string.Join(",", sLoopVoice30CustomMune[i][j]));
                        }
                    }

                    if (i < sLoopVoice30CustomKokan.Length)
                    {
                        for (int j = 0; j < sLoopVoice30CustomKokan[i].Length; j++)
                        {
                            Console.WriteLine(" CustomKokan [" + i + "][" + j + "] " +
                                              string.Join(",", sLoopVoice30CustomKokan[i][j]));
                        }
                    }
                }
            }
        }

        //ユニーク名と部位から音声をReflectionで取得 カスタムボイスIDが指定されていればそちらから取得
        public string[][] getLoopVoice(string uniqueName, string target, int customId)
        {
            //Console.WriteLine("[KissYourMaid] getLoopVoice uniqueName="+uniqueName+" target="+target+" customId="+customId);
            if (customId != -1)
            {
                //カスタムボイスの場合
                if (target == "Excite")
                {
                    if (sLoopVoice30CustomExcite.Length > customId) return sLoopVoice30CustomExcite[customId];
                }
                else if (target == "Mune")
                {
                    if (sLoopVoice30CustomMune.Length > customId) return sLoopVoice30CustomMune[customId];
                }
                else if (target == "Kokan")
                {
                    if (sLoopVoice30CustomKokan.Length > customId) return sLoopVoice30CustomKokan[customId];
                }
            }
            else
            {
                //性格別ボイス
                FieldInfo voiceFieldInfo = typeof(KissYourMaidConfig).GetField("sLoopVoice30" + uniqueName + target,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField);
                if (voiceFieldInfo != null) return (string[][])voiceFieldInfo.GetValue(cfg);
            }

            return null;
        }

        //　状態管理
        private int iState = 0; //　ステート番号
        private int iStateMajor = 10; //　距離によるステート
        private int iStateMajorOld = 10; //　距離によるステート（前回値）
        private int iStateMinor = 0; //　時間経過によるステート

        //　10 …　離れている
        //　20 …　近い（キス前・遷移直後）
        //　21 …　近い（キス前・一定時間経過後）
        //　30 …　とても近い（キス）
        //　40 …　近い（キス後・遷移直後）
        //　41 …　近い（キス後・一定時間経過後）

        //　距離判定
        //private float fDistanceToMaidFace;
        private float fDistanceThreshold1;
        private float fDistanceThreshold2;

        //　表情管理
        private int iStateHoldTime;
        private int iStateAltTime1;
        private int iStateAltTime2;
        private string sFaceAnimeBackup = "";
        private string sFaceBlendBackup = "";

        //　声管理
        private bool bIsVoiceOverriding = false; //　音声オーバライド（上書き）を適用中
        private string sLoopVoiceOverriding = ""; //　音声オーバライド（上書き）を適用している音声ファイル名
        private float fLoopVoiceEndTime = 0f; //  再生中のループ音声の終了時間
        private bool bOverrideInterrupted = false; //　音声オーバライド（上書き）を適用したが、スキル変更などにより割りこまれた
        private string sLoopVoiceBackup = ""; //　音声オーバライド（上書き）を終了した時に、復元再生する音声ファイル名

        //　興奮度管理
        private int iExciteLevel = 1; //　０～３００の興奮度を、興奮レベルに変換した値（１～４）
        private int iExciteLevelMod = 0; //　興奮レベルに対する手動変更値（－３～＋３）

        private Maid playingMaid = null; //ボイスを再生しているメイド
        private string playingTargetPart = ""; //ボイスを再生している部位
        private int playingExciteLevel = -1; //ボイスを再生している興奮レベル

        //　Chu-B Lip / VR
        private bool bChuBLip;
        private bool bOculusVR;
        private int iExciteLevelOld;
        private bool bPistonSpeedChanged;

        //　フェラ状態チェック
        bool bIsBlowjobing = false;
        private string sLastAnimeFileName = "";
        private string sLastAnimeFileNameOld = "";

        //
        private int iSceneLevel = 0;
        private int iPrevSceneLevel = 0;
        private bool bIsYotogiScene;
        private bool bIsEditScene;

        private int iFrameCount = 0;
        private int iFpsMax = 60;
        private int iKeyHoldCount = 0;

        //
        private CameraMain mainCamera;
        private Maid maid;
        private Transform maidHead;
        private Transform maidMuneL;
        private Transform maidMuneR;
        private Transform maidKokan;
        private Transform maidFootL;
        private Transform maidFootR;
        private Transform maidBip;

        //
        private string sTargetMaidPart;
        private string sTargetMaidPartOld;
        private float fDistanceToTarget;
        private int iConvulsionTimeCounter = -1;

        //複数人対応
        private int iTargetMaid = -1;

        //private int iStockMaidCount = 999;
        private bool bTargetMaidChanged = false;


        private GameObject gearMenuButton;
        private bool buttonToggled = false;


        KissYourMaidConfig cfg = new KissYourMaidConfig();

        public void Awake()
        {
            //シーンロード登録
            //SceneManager.activeSceneChanged += OnActiveSceneChanged;

            GameObject.DontDestroyOnLoad(this);

            // Iniファイル読み込み
            string configPath = getConfigPath();
            if (System.IO.File.Exists(configPath))
            {
                cfg = SharedConfig.ReadConfig<KissYourMaidConfig>("Config", configPath);
                Console.WriteLine("[KissYourMaid] Config Loaded");
            }
            else
            {
                // Iniファイル書き出し ファイルがなければ
                SharedConfig.SaveConfig("Config", configPath, cfg);
                Console.WriteLine("[KissYourMaid] Config Saved");
            }

            string path = UnityEngine.Application.dataPath;
            // ChuBLip判別
            bChuBLip = path.Contains("COM3D2OHx64") || path.Contains("COM3D2OHx86") || path.Contains("COM3D2OHVRx64");
            // VR判別
            bOculusVR = path.Contains("COM3D2OHVRx64") || path.Contains("COM3D2VRx64") ||
                        Environment.CommandLine.ToLower().Contains("/vr");
            if (bOculusVR)
            {
                cfg.fDistanceThresholdBase1 = cfg.fDistanceThresholdBase1VR;
                cfg.fDistanceThresholdBase2 = cfg.fDistanceThresholdBase2VR;
                cfg.fDistanceThresholdOffset = cfg.fDistanceThresholdOffsetVR;
                iFpsMax = cfg.iFpsMaxVR;
            }
            else
            {
                iFpsMax = cfg.iFpsMax;
            }

            fDistanceThreshold1 = cfg.fDistanceThresholdBase1;
            fDistanceThreshold2 = cfg.fDistanceThresholdBase2;

            //カスタムボイス変換 設定によってはエラーが出る可能性があるので最後に実行
            setCustomVoice();

            Console.WriteLine("[KissYourMaid] Awake bChuBLip=" + bChuBLip + " bOculusVR=" + bOculusVR);
        }

        public void Start()
        {
        }

        public void OnDestroy()
        {
            // Iniファイル書き出し cfgへの更新分を反映
            SharedConfig.SaveConfig("Config", getConfigPath(), cfg);
        }

        private string getConfigPath()
        {
            return System.IO.Path.Combine(
                System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "Config"),
                GetType().Name + ".ini"
            );
        }


        void OnLevelWasLoaded(int level)
        {
            //　レベルの取得
            iSceneLevel = level;
            Console.WriteLine("[KissYourMaid] OnActiveSceneChanged " + iSceneLevel);

            //　メインカメラの取得
            mainCamera = GameMain.Instance.MainCamera;

            //　メイドさんの取得
            maid = GameMain.Instance.CharacterMgr.GetMaid(0);

            // 夜伽シーンに有るかチェック
            checkYotogiScene();

            // エディットシーンに有るかチェック
            checkEditScene();

            //ダンス以外の他のシーンでも有効にする
            if (!bIsEditScene && !bIsYotogiScene)
            {
                //シーン15を有効にする
                if (cfg.bPluginValidADV && iSceneLevel == 15) bIsYotogiScene = true;

                //ホーム + イベント
                if (cfg.bPluginValidEvent)
                {
                    //回想 （Hイベント）
                    if (iSceneLevel == 24) bIsYotogiScene = true;
                }

                //シーン15設定と異なる場合は有効状態変更
                if (cfg.bPluginValidADV != cfg.bPluginValidEvent)
                {
                    //Hイベント&夜伽前会話:1→15 / ライフモード:81→15 / 訓練イベント:15→15 / イベント選択からの再生:42→15
                    if (iSceneLevel == 15 && (iPrevSceneLevel == 1 || iPrevSceneLevel == 81 || iPrevSceneLevel == 15 ||
                                              iPrevSceneLevel == 42)) bIsYotogiScene = cfg.bPluginValidEvent;
                }

                //プライベート + 嫁イド
                if (cfg.bPluginValidPrivate)
                {
                    //ホーム:3 / 夜伽前会話:14 / プライベートモード:95
                    if (iSceneLevel == 3 || iSceneLevel == 14 || iSceneLevel == 95) bIsYotogiScene = true;
                }

                //シーン15設定と異なる場合は有効状態変更
                if (cfg.bPluginValidADV != cfg.bPluginValidPrivate)
                {
                    //プライベートモード:95→15 / 嫁イド挨拶(夜):3→15 / 夜伽後会話:14→15 /  嫁イド挨拶(朝):16→15
                    if (iSceneLevel == 15 && (iPrevSceneLevel == 95 || iPrevSceneLevel == 3 || iPrevSceneLevel == 14 ||
                                              iPrevSceneLevel == 16)) bIsYotogiScene = cfg.bPluginValidPrivate;
                }

                //ゲストモード
                if (cfg.bPluginValidGuest)
                {
                    //ゲストモード:53
                    if (iSceneLevel == 53) bIsYotogiScene = true;
                }
            }

            //　各変数の初期化
            initTempVariables();


            //　歯車ボタンの作成と除去
            if (bIsYotogiScene || bIsEditScene)
            {
                //　作成
                if (!gearMenuButton)
                {
                    List<UIAtlas> uiAtlas = new List<UIAtlas>();
                    uiAtlas.AddRange(Resources.FindObjectsOfTypeAll<UIAtlas>());
                    UIAtlas uiAtlasPreset = uiAtlas.FirstOrDefault(a => a.name == "AtlasPreset");

                    // GearMenuを利用してシステムメニューにボタン追加
                    if ((bIsYotogiScene && cfg.bPluginEnabled) ||
                        (bIsEditScene && cfg.bPluginEnabledInEdit && cfg.bPluginEnabled))
                    {
                        gearMenuButton = GearMenu.Buttons.Add("KissYourMaid", "KissYourMaid", KissEnableIcon.Png,
                            (go) => togglePluginEnabled());
                        GearMenu.Buttons.SetText(gearMenuButton, "KissYourMaid is ON");
                        buttonToggled = true;
                    }
                    else
                    {
                        gearMenuButton = GearMenu.Buttons.Add("KissYourMaid", "KissYourMaid", KissDisableIcon.Png,
                            (go) => togglePluginEnabled());
                        GearMenu.Buttons.SetText(gearMenuButton, "KissYourMaid is OFF");
                        buttonToggled = false;
                    }
                }
            }
            else
            {
                //　除去
                if (gearMenuButton)
                {
                    GearMenu.Buttons.Remove(gearMenuButton);
                }
            }

            if (iPrevSceneLevel != iSceneLevel) iPrevSceneLevel = iSceneLevel;

            Console.WriteLine("[KissYourMaid] YotogiScene=" + bIsYotogiScene + " EditScene=" + bIsEditScene);
        }

        void initTempVariables()
        {
            sFaceAnimeBackup = "";
            sFaceBlendBackup = "";
            bIsVoiceOverriding = false;
            sLoopVoiceOverriding = "";
            bOverrideInterrupted = false;
            sLoopVoiceBackup = "";
            iExciteLevel = 1;
            iExciteLevelMod = 0;
            bIsBlowjobing = false;
            sLastAnimeFileName = "";
            sLastAnimeFileNameOld = "";
            sTargetMaidPart = "";
            sTargetMaidPartOld = "";
            iConvulsionTimeCounter = -1;
        }


#if DEBUG
        void OnGUI()
        {
                //GUIへのデバッグ表示
                if (maid) {
                    GUI.Label(new Rect(0, 0, 900, 30),
                                  string.Format("Level={0}, Yotogi={1}, iState={2}, fDistanceToTarget={3}, sLastAnimeFileName={4}, sTargetMaidPart={5} ",
                                                 iSceneLevel, bIsYotogiScene, iState, fDistanceToTarget, sLastAnimeFileName, sTargetMaidPart));

                    if (maid.AudioMan != null && maid.AudioMan.FileName != null) {
                        GUI.Label(new Rect(0, 30, 900, 60),
                                      string.Format("AudioClip={0}, IsLoop={1}, AudioTime={2} of {3}, ActiveFace={4}, ExciteLevel={5}",
                                                     maid.AudioMan.FileName, maid.AudioMan.audiosource.loop, maid.AudioMan.audiosource.time, maid.AudioMan.audiosource.clip.length, maid.ActiveFace, iExciteLevel));

                        GUI.Label(new Rect(0, 60, 900, 90),
                                      string.Format("bIsVoiceOverriding={0}, sLoopVoiceBackup={1}, iStateAltTime1={2}, iStateAltTime2={3}",
                                                    bIsVoiceOverriding, sLoopVoiceBackup, iStateAltTime1, iStateAltTime2));
                    }
                }
        }
#endif


        void Update()
        {
            // トグルキーの長押しカウント
            if (Input.GetKeyDown(cfg.keyPluginToggle))
            {
                iKeyHoldCount = 0;
            }

            //　本プラグインの有効無効の切替
            if (Input.GetKeyUp(cfg.keyPluginToggle))
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    //　Ctrlと一緒に押されたら不機嫌モードのトグル
                    cfg.bFukigenEnabled = !cfg.bFukigenEnabled;
                    if (cfg.bFukigenEnabled)
                    {
                        GameMain.Instance.SoundMgr.PlaySe("SE001.ogg", false);
                        Console.WriteLine("[KissYourMaid] Fukigen Mode Enabled.");
                    }
                    else
                    {
                        GameMain.Instance.SoundMgr.PlaySe("SE002.ogg", false);
                        Console.WriteLine("[KissYourMaid] Fukigen Mode Disabled.");
                    }
                }
                else
                {
                    //　短押しならプラグイン有効無効をトグルする（長押しはパターンor興奮レベル手動変更）
                    //　１秒間のフレームの1/3…約0.33秒
                    if (iKeyHoldCount < (iFpsMax / 3))
                    {
                        togglePluginEnabled();
                    }
                }
            }

            if (!bGlobalPluginEnabled)
            {
                return;
            }

            //　プラグインが有効なシーンであるか判別する
            //　夜伽シーン・エディットモード・撮影モードのいずれかにあり、プラグインがEnabledであること（CBLはエディットが4？）
            if ((bIsYotogiScene && cfg.bPluginEnabled) ||
                (bIsEditScene && cfg.bPluginEnabledInEdit && cfg.bPluginEnabled))
            {
                //　歯車ボタンの更新
                setButtonState(true);
            }
            else
            {
                //　歯車ボタンの更新
                setButtonState(false);
                //プラグインが有効なシーンでなければ、ここで抜けて以後の処理を行わない
                return;
            }

            //オーバーライド中の音声の再生が終了したら即再生させる
            if (bIsVoiceOverriding && !(maid.AudioMan && maid.AudioMan.audiosource.isPlaying) && iFrameCount != iFpsMax)
            {
                //音声終了時に即次の音声再生させる
                iFrameCount = iFpsMax;
            }

            //　約１秒毎に呼び出す
            if (iFrameCount == iFpsMax)
            {
                iFrameCount = 0;

                //フェードアウト中は処理しない
                if (!GameMain.Instance.MainCamera.IsFadeStateNon()) return;

                //　ストップウォッチを開始する（処理時間計測用）
                //Stopwatch sw = new Stopwatch();
                //sw.Start();

                //　もっとも近いメイドさんを選択する
                checkNearestMaid();
                //　メイドさんが有効かチェック
                checkTargetMaid();
                //　メイドさんの部位を取得
                getMaidObject();

                //　有効なメイドさんがおり、Bone_Face等が取得済みであること
                if (iTargetMaid != -1 && maid && maid.ActiveSlotNo != -1 && maidHead)
                {
                    //  フェラ判定
                    checkBlowjobing();
                    if (!bIsBlowjobing)
                    {
                        //  表情等変更処理を実施する
                        checkFaceDistance();
                    }
                }

                //　ストップウォッチを止める
                //sw.Stop();
                //debugPrintConsole("Stopwatch: " + sw.Elapsed.ToString());
            }
            else
            {
                iFrameCount++;
            }

            //　痙攣処理
            if (cfg.bConvulsionEnabled)
            {
                if (maid && maid.ActiveSlotNo != -1 && maidHead)
                {
                    //　痙攣の時間カウント処理（毎フレーム処理）
                    convulsionCounter();
                    //　痙攣の開始処理（０．５秒毎）
                    if (iFrameCount == iFpsMax * 0.5f)
                    {
                        convulsionStartCheck();
                    }
                }
            }


            //　パターン手動変更操作
            if (Input.GetKey(cfg.keyPluginToggle))
            {
                if (Input.GetKeyDown(cfg.keyManualForceAlt))
                {
                    //　パターン手動変更
                    iStateHoldTime = 0; //ステート保持時間をリセットすることで再適用を促す
                    iKeyHoldCount = iFpsMax; //プラグインON/OFFをタイムアウトさせる
                }
                else if (Input.GetKeyDown(cfg.keyManualIncrease))
                {
                    //　興奮レベル手動変更＋
                    iExciteLevelMod++;
                    if (iExciteLevel + iExciteLevelMod <= 4) iStateHoldTime = 0; //上下限値を越えようとしてなければ再適用する
                    iKeyHoldCount = iFpsMax; //プラグインON/OFFをタイムアウトさせる
                }
                else if (Input.GetKeyDown(cfg.keyManualDecrease))
                {
                    //　興奮レベル手動変更－
                    iExciteLevelMod--;
                    if (1 <= iExciteLevel + iExciteLevelMod) iStateHoldTime = 0; //上下限値を越えようとしてなければ再適用する
                    iKeyHoldCount = iFpsMax; //プラグインON/OFFをタイムアウトさせる
                }

                //押しっぱなしフレーム数のカウント（プラグインON/OFFのタイムアウト用）
                iKeyHoldCount++;
            }
        }


        /*
        //　次の有効なメイドさんを探す
        private Maid getNextMaid()
        {
            if (iTargetMaid != -1)
            {
                for (int i = 0; i < iStockMaidCount; i++)
                {
                    //　次のメイドさん番号
                    int j = iTargetMaid + i + 1;
                    if (iStockMaidCount <= j) j -= iStockMaidCount;

                    //　有効なメイドで表示されている
                    Maid tempMaid = GameMain.Instance.CharacterMgr.GetStockMaidList()[j];
                    if (tempMaid)
                    {
                        if (tempMaid.Visible == true)
                        {
                            iTargetMaid = j;
                            return tempMaid;
                        }
                    }
                }
            }
            return null;
        }


        //　前の有効なメイドさんを探す
        private Maid getPrevMaid()
        {
            //　メイドさんが少なくとも一人存在すること
            if (iTargetMaid != -1)
            {
                for (int i = 0; i < iStockMaidCount; i++)
                {
                    //　次のメイドさん番号
                    int j = iTargetMaid - i - 1;
                    if (j <= -1) j += iStockMaidCount;

                    //　有効なメイドで表示されている
                    Maid tempMaid = GameMain.Instance.CharacterMgr.GetStockMaidList()[j];
                    if (tempMaid)
                    {
                        if (tempMaid.Visible == true)
                        {
                            iTargetMaid = j;
                            return tempMaid;
                        }
                    }
                }
            }
            return null;
        }
        */


        //　注視対象のメイドさんの有効状態をチェックする
        private void checkTargetMaid()
        {
            //　メイドさんの総数
            //iStockMaidCount = GameMain.Instance.CharacterMgr.GetStockMaidCount();

            //　メイドさんが無効もしくは消えたら選択状態を初期化する
            if (iTargetMaid != -1)
            {
                if (!maid)
                {
                    iTargetMaid = -1;
                }
                else
                {
                    if (!maid.Visible)
                    {
                        iTargetMaid = -1;
                    }
                }
            }

            //　メイドさんが選択されてなければ、適当にインデックスの小さいものから探す
            if (iTargetMaid == -1)
            {
                int i = 0;
                foreach (Maid tempMaid in GameMain.Instance.CharacterMgr.GetStockMaidList())
                {
                    if (tempMaid != null)
                    {
                        if (tempMaid.Visible == true)
                        {
                            iTargetMaid = i;
                            maid = tempMaid;
                            bTargetMaidChanged = true;
                            break;
                        }
                    }

                    i++;
                }
            }
        }

        //　主観視点のカメラ位置を返す
        private Transform getMainCameraPos()
        {
            if (bOculusVR)
            {
                return GameMain.Instance.OvrMgr.EyeAnchor;
                /*try
                {
                    Transform vrCameraPos = Util.GetObject.ByString("GameMain.m_objInstance.m_OvrMgr.m_trEyeAnchor") as Transform;
                    cameraPos = vrCameraPos;
                }
                catch (Exception ex)
                {
                    debugPrintConsole(ex.ToString());
                }*/
            }

            return mainCamera.transform;
        }


        //　一番近い有効なメイドさんをチェックする
        private void checkNearestMaid()
        {
            //　メイドさんの総数
            //iStockMaidCount = GameMain.Instance.CharacterMgr.GetStockMaidCount();

            //「離れている」の時だけ変更する（キス状態に有る時は、一度離れて復元処理をして解放してから、他のメイドさんに切り替えられるようにする）
            if (iStateMajor == 10 && !bIsVoiceOverriding)
            {
                iTargetMaid = -1;
                float fDistanceToNearestMaid = 1234.0f;
                float fDistanceToTempMaid = 1234.0f;

                //　主観視点のカメラ位置を取得
                Transform cameraPos = getMainCameraPos();

                //　表示中のメイドさんを全てチェックする
                int i = 0;
                foreach (Maid tempMaid in GameMain.Instance.CharacterMgr.GetStockMaidList())
                {
                    if (tempMaid != null)
                    {
                        if (tempMaid.Visible == true)
                        {
                            //　メイドさんの部位でカメラと一番近いものとの距離を取得
                            fDistanceToTempMaid = getDistanceToNearestPart(cameraPos, tempMaid);

                            //　より近いメイドさんを対象にする
                            if (fDistanceToTempMaid < fDistanceToNearestMaid)
                            {
                                fDistanceToNearestMaid = fDistanceToTempMaid;
                                iTargetMaid = i;
                                maid = tempMaid;
                                bTargetMaidChanged = true;
                                debugPrintConsole("NearestMaid: " + iTargetMaid.ToString() + " Distance: " +
                                                  fDistanceToNearestMaid.ToString());
                            }
                        }
                    }

                    i++;
                }
            }
        }

        //　メイドさんの部位でカメラと一番近いものとの距離を取得
        public float getDistanceToNearestPart(Transform cameraPos, Maid tempMaid)
        {
            float fDistanceToNearestPart = 1234.0f;

            Transform[] objList = tempMaid.transform.GetComponentsInChildren<Transform>();
            if (objList.Count() == 0)
            {
            }
            else
            {
                maidHead = null;
                maidBip = null;
                maidMuneL = null;
                maidMuneR = null;
                maidKokan = null;
                maidFootL = null;
                maidFootR = null;

                foreach (var gameobject in objList)
                {
                    if (gameobject.name == "Bone_Face" && maidHead == null)
                    {
                        maidHead = gameobject;
                        fDistanceToNearestPart = Math.Min(fDistanceToNearestPart,
                            Vector3.Distance(gameobject.position, cameraPos.position));
                    }
                    else if (gameobject.name == "Bip01" && maidBip == null)
                    {
                        maidBip = gameobject;
                        fDistanceToNearestPart = Math.Min(fDistanceToNearestPart,
                            Vector3.Distance(gameobject.position, cameraPos.position));
                    }
                    else if (gameobject.name == "Mune_L_sub" && maidMuneL == null)
                    {
                        maidMuneL = gameobject;
                        fDistanceToNearestPart = Math.Min(fDistanceToNearestPart,
                            Vector3.Distance(gameobject.position, cameraPos.position));
                    }
                    else if (gameobject.name == "Mune_R_sub" && maidMuneR == null)
                    {
                        maidMuneR = gameobject;
                        fDistanceToNearestPart = Math.Min(fDistanceToNearestPart,
                            Vector3.Distance(gameobject.position, cameraPos.position));
                    }
                    else if (gameobject.name == "_IK_vagina" && maidKokan == null)
                    {
                        maidKokan = gameobject;
                        fDistanceToNearestPart = Math.Min(fDistanceToNearestPart,
                            Vector3.Distance(gameobject.position, cameraPos.position));
                    }
                    else if (cfg.bTargetFoots)
                    {
                        if (gameobject.name == "Bip01 L Foot" && maidFootL == null)
                        {
                            maidFootL = gameobject;
                            fDistanceToNearestPart = Math.Min(fDistanceToNearestPart,
                                Vector3.Distance(gameobject.position, cameraPos.position));
                        }

                        if (gameobject.name == "Bip01 R Foot" && maidFootR == null)
                        {
                            maidFootR = gameobject;
                            fDistanceToNearestPart = Math.Min(fDistanceToNearestPart,
                                Vector3.Distance(gameobject.position, cameraPos.position));
                        }
                    }
                }
            }

            return fDistanceToNearestPart;
        }


        //　有効なメイドさんを探し、顔や胸などのオブジェクトを特定しておく
        private void getMaidObject()
        {
            //　メイドさんの顔が未取得なら取得する
            //（エディットで顔が変わった時、Bone_Faceを再取得する必要があるので毎秒処理に変更 151215）
            if (maid)
            {
                if (maid.Visible)
                {
                    if (!maidHead || bTargetMaidChanged)
                    {
                        Transform[] objList = maid.transform.GetComponentsInChildren<Transform>();
                        if (objList.Count() == 0)
                        {
                        }
                        else
                        {
                            maidHead = null;
                            maidBip = null;
                            maidMuneL = null;
                            maidMuneR = null;
                            maidKokan = null;
                            maidFootL = null;
                            maidFootR = null;

                            foreach (var gameobject in objList)
                            {
                                if (gameobject.name == "Bone_Face" && maidHead == null)
                                {
                                    maidHead = gameobject;
                                }
                                else if (gameobject.name == "Bip01" && maidBip == null)
                                {
                                    maidBip = gameobject;
                                }
                                else if (gameobject.name == "Mune_L_sub" && maidMuneL == null)
                                {
                                    maidMuneL = gameobject;
                                }
                                else if (gameobject.name == "Mune_R_sub" && maidMuneR == null)
                                {
                                    maidMuneR = gameobject;
                                }
                                else if (gameobject.name == "_IK_vagina" && maidKokan == null)
                                {
                                    maidKokan = gameobject;
                                }
                                else if (gameobject.name == "Bip01 L Foot" && maidFootL == null)
                                {
                                    maidFootL = gameobject;
                                }
                                else if (gameobject.name == "Bip01 R Foot" && maidFootR == null)
                                {
                                    maidFootR = gameobject;
                                }
                            }
                        }
                    }
                }
            }
        }


        //　歯車ボタンの状態を切り替える 状態に変更があったときのみ処理
        private void setButtonState(bool toggled)
        {
            if (buttonToggled != toggled)
            {
                buttonToggled = toggled;

                if (gearMenuButton == null) return;

                UITexture componentInChildren = gearMenuButton.GetComponentInChildren<UITexture>();
                Texture2D texture2D = componentInChildren.mainTexture as Texture2D;

                if (toggled)
                {
                    GearMenu.Buttons.SetText(gearMenuButton, "KissYourMaid is ON");
                    texture2D.LoadImage(KissEnableIcon.Png);
                }
                else
                {
                    GearMenu.Buttons.SetText(gearMenuButton, "KissYourMaid is OFF");
                    texture2D.LoadImage(KissDisableIcon.Png);
                }

                texture2D.Apply();
            }
        }


        private void updateGearMenuButton()
        {
            if (gearMenuButton == null) return;

            UITexture componentInChildren = gearMenuButton.GetComponentInChildren<UITexture>();
            Texture2D texture2D = componentInChildren.mainTexture as Texture2D;

            if (bGlobalPluginEnabled)
            {
                GearMenu.Buttons.SetText(gearMenuButton, "KissYourMaid is ON");
                texture2D.LoadImage(KissEnableIcon.Png);
            }
            else
            {
                GearMenu.Buttons.SetText(gearMenuButton, "KissYourMaid is OFF");
                texture2D.LoadImage(KissDisableIcon.Png);
            }

            texture2D.Apply();
        }


        //　プラグイン有効状態の変更と、プラグイン無効化時の処理
        private void togglePluginEnabled()
        {
            //　プラグイン有効状態の変更
            bGlobalPluginEnabled = !bGlobalPluginEnabled;

            // //　夜伽シーン
            // if (bIsYotogiScene)
            // {
            //     cfg.bPluginEnabled = !cfg.bPluginEnabled;
            //     if (cfg.bPluginEnabled)
            //     {
            //         Console.WriteLine("[KissYourMaid] Plugin Enabled.");
            //     }
            //     else
            //     {
            //         Console.WriteLine("[KissYourMaid] Plugin Disabled.");
            //     }
            //
            // }
            //
            // //　エディットor撮影シーン
            // if (bIsEditScene)
            // {
            //     cfg.bPluginEnabledInEdit = !cfg.bPluginEnabledInEdit;
            //     if (cfg.bPluginEnabledInEdit)
            //     {
            //         Console.WriteLine("[KissYourMaid] Plugin Enabled.(EditMode)");
            //     }
            //     else
            //     {
            //         Console.WriteLine("[KissYourMaid] Plugin Disabled.(EditMode)");
            //     }
            //
            // }


            // プラグイン無効化時の処理
            if (sFaceAnimeBackup != "")
            {
                if (!bGlobalPluginEnabled) // プラグインが無効化にされた場合
                {
                    //　痙攣用のアニメーション速度を戻してカウンタを無効化
                    AnimationState state = this.GetCurrentAnimationState(maid);
                    if (state != null)
                    {
                        state.speed = 1.0f;
                        iConvulsionTimeCounter = -1;
                    }

                    //　撮影モードで誰もいない時はスキップ
                    if (iTargetMaid != -1)
                    {
                        //　表情の復元
                        restoreFace();

                        //　顔の追従状態を変更する（できればこちらも復元したいが現況取得がわからない）
                        //　顔の追従は切る　…　フェラ時にズレることがないように、
                        //　眼の追従は残す　…　キス直後に明後日の方向を見られると寂しいので
                        maid.EyeToCamera(Maid.EyeMoveType.目だけ向ける, cfg.fAnimeFadeTime);

                        //　音声の復元もしくは停止
                        if (bIsVoiceOverriding)
                        {
                            //　ステートをリセット
                            iStateMajor = 10;
                            iStateMinor = 0;
                            iStateMajorOld = 10;

                            //　オーバライド状態を解除
                            bIsVoiceOverriding = false;
                            bOverrideInterrupted = false;

                            //　復元もしくは停止
                            if (sLoopVoiceBackup != "")
                            {
                                maid.AudioMan.LoadPlay(sLoopVoiceBackup, 0f, false, true);
                                debugPrintConsole("voice restore done. " + sLoopVoiceBackup);
                            }
                            else
                            {
                                maid.AudioMan.Stop();
                                debugPrintConsole("voice stop done. " + sLoopVoiceBackup);
                            }
                        }
                    }

                    //　一時変数の初期化
                    initTempVariables();
                }
            }
        }


        //　痙攣処理の時間カウント
        private void convulsionCounter()
        {
            if (iConvulsionTimeCounter > 0)
            {
                iConvulsionTimeCounter--;
            }
            else if (iConvulsionTimeCounter == 0)
            {
                //　痙攣用のアニメーション速度を戻してカウンタを無効化
                AnimationState state = this.GetCurrentAnimationState(maid);
                if (state != null)
                {
                    state.speed = 1.0f;
                    iConvulsionTimeCounter = -1;
                }
            }
        }


        //　ランダムで痙攣させる
        private void convulsionStartCheck()
        {
            //　とても近く、口以外へのキスの時だけ有効にする
            if (iStateMajor == 30 && sTargetMaidPart != "maidHead")
            {
                //　興奮度に応じて痙攣確率を変える
                int iConvulsionChancePercent = 0;
                switch (iExciteLevel + iExciteLevelMod)
                {
                    case 1:
                        iConvulsionChancePercent = cfg.iConvulsionChancePercentExcite1;
                        break;
                    case 2:
                        iConvulsionChancePercent = cfg.iConvulsionChancePercentExcite2;
                        break;
                    case 3:
                        iConvulsionChancePercent = cfg.iConvulsionChancePercentExcite3;
                        break;
                    case 4:
                        iConvulsionChancePercent = cfg.iConvulsionChancePercentExcite4;
                        break;
                }

                //  確率で痙攣させる
                int r = UnityEngine.Random.Range(1, 100);
                if (r <= iConvulsionChancePercent)
                {
                    //　現在のアニメ－ションの再生スピードを変更する
                    //　（スピードと変更時間はランダムで幅を持たせる）
                    AnimationState state = this.GetCurrentAnimationState(maid);
                    if (state != null)
                    {
                        state.speed = cfg.fConvulsionSpeedMultiplierBase +
                                      UnityEngine.Random.Range(0f, cfg.fConvulsionSpeedMultiplierRandomExtend);
                        iConvulsionTimeCounter = cfg.iConvulsionTimeFramesBase +
                                                 UnityEngine.Random.Range(0, cfg.iConvulsionTimeFramesBaseRandomExtend);
                        ;
                    }
                }
            }
        }


        //　カメラ（プレイヤーの視点）とメイドさんの頭との距離に応じて、
        //　表情など（フェイスアニメ、フェイスブレンド、顔目追従の状態）を変える
        private void checkFaceDistance()
        {
            //　---- 距離判定とターゲット確定 ----------------------------------------------------

            //　主観視点のカメラ位置を取得
            Transform cameraPos = getMainCameraPos();


            //　主観視点と近接判定対象（ターゲット）の距離取得
            float fDistanceToMaidHead = Vector3.Distance(maidHead.transform.position, cameraPos.position);
            fDistanceToTarget = fDistanceToMaidHead;
            sTargetMaidPart = "maidHead";

            if (cfg.bTargetOtherParts) //  口以外のパーツをターゲットにするのが有効な場合
            {
                //　胸への距離
                if (cfg.bTargetMune)
                {
                    float fDistanceToMaidMune =
                        Math.Min(Vector3.Distance(maidMuneL.transform.position, cameraPos.position),
                            Vector3.Distance(maidMuneR.transform.position, cameraPos.position));
                    //口を近く判定
                    if (fDistanceToMaidMune < fDistanceToTarget + cfg.fCheckOffsetHead)
                    {
                        //ステート判定用の距離を補正 胸サイズが小さいほど距離が離れている (0:+15cm 50:+10cm 100:+5cm 150:0cm) + 胸オフセット
                        float muneSize = (float)(150 - maid.GetProp(MPN.MuneL).value) / 1000f;
                        fDistanceToTarget = fDistanceToMaidMune + muneSize + cfg.fDistanceOffsetMune;
                        sTargetMaidPart = "maidMune";
                    }
                }

                //　股間への判定
                if (cfg.bTargetKokan)
                {
                    float fDistanceToMaidKokan = Vector3.Distance(maidKokan.transform.position, cameraPos.position);
                    if (fDistanceToMaidKokan < fDistanceToTarget)
                    {
                        fDistanceToTarget = fDistanceToMaidKokan + cfg.fDistanceOffsetKokan;
                        sTargetMaidPart = "maidKokan";
                    }
                }

                //　足への判定
                if (cfg.bTargetFoots)
                {
                    float fDistanceToMaidFootL = Vector3.Distance(maidFootL.transform.position, cameraPos.position);
                    if (fDistanceToMaidFootL < fDistanceToTarget)
                    {
                        fDistanceToTarget = fDistanceToMaidFootL + cfg.fDistanceOffsetFoot;
                        sTargetMaidPart = "maidFootL";
                    }

                    float fDistanceToMaidFootR = Vector3.Distance(maidFootR.transform.position, cameraPos.position);
                    if (fDistanceToMaidFootR < fDistanceToTarget)
                    {
                        fDistanceToTarget = fDistanceToMaidFootR + cfg.fDistanceOffsetFoot;
                        sTargetMaidPart = "maidFootR";
                    }
                }
            }


            //　---- ステート管理 ----------------------------------------------------

            //　距離ステートの変更
            if (fDistanceToTarget < fDistanceThreshold2)
            {
                //　「とても近い」
                iStateMajor = 30;
                fDistanceThreshold1 = cfg.fDistanceThresholdBase1 + cfg.fDistanceThresholdOffset;
                fDistanceThreshold2 = cfg.fDistanceThresholdBase2 + cfg.fDistanceThresholdOffset;
            }
            else if (fDistanceToTarget < fDistanceThreshold1)
            {
                //　「近い」
                //　「離れている」から来た時と、「とても近い」から来た時を区別する
                if (iStateMajorOld < 20)
                {
                    iStateMajor = 20;
                }
                else if (20 <= iStateMajorOld && iStateMajorOld < 30)
                {
                    iStateMajor = 20;
                }
                else if (30 <= iStateMajorOld && iStateMajorOld < 40)
                {
                    iStateMajor = 40;
                }
                else if (40 <= iStateMajorOld)
                {
                    iStateMajor = 40;
                }

                fDistanceThreshold1 = cfg.fDistanceThresholdBase1 + cfg.fDistanceThresholdOffset;
                fDistanceThreshold2 = cfg.fDistanceThresholdBase2;
            }
            else
            {
                //　「離れている」
                iStateMajor = 10;
                fDistanceThreshold1 = cfg.fDistanceThresholdBase1;
                fDistanceThreshold2 = cfg.fDistanceThresholdBase2;
            }

            //　「離れている」から遷移してくる時には、その時点での表情などをバックアップしておく
            if (iStateMajor != 10 && iStateMajorOld == 10) backupFace();

            //　距離ステートが変わったら、もしくはターゲット部位が変わったら、時間カウンタをリセットする
            if (iStateMajor != iStateMajorOld || sTargetMaidPart != sTargetMaidPartOld)
            {
                //　「近い」では時間カウンタのリセットをしない
                if (iStateMajorOld != 20 && iStateMajorOld != 40)
                {
                    //　時間カウンタのリセット
                    iStateHoldTime = 0;
                    //　表情変化時間のランダマイズ
                    iStateAltTime1 = cfg.iStateAltTime1Base +
                                     UnityEngine.Random.Range(0, cfg.iStateAltTime1RandomExtend + 1);
                }
            }

            //　時間ステートの変更
            if (iStateMajor == 10)
            {
                iStateMinor = 0;
                iExciteLevelMod = 0; //興奮値の手動変更値もリセットする
            }

            else if (iStateMajor == 20 || iStateMajor == 40)
            {
                //　時間経過により、一方通行で表情変化
                iStateMinor = 0;
                if (iStateAltTime1 <= iStateHoldTime) iStateMinor = 1;
            }

            else if (iStateMajor == 30)
            {
                iStateMinor = 0;
                if (iStateAltTime2 <= iStateHoldTime)
                {
                    //　時間カウンタのリセット
                    iStateHoldTime = 0;
                    //　表情変化時間のランダマイズ
                    iStateAltTime2 = cfg.iStateAltTime2Base +
                                     UnityEngine.Random.Range(0, cfg.iStateAltTime2RandomExtend + 1);
                }
            }

            iState = iStateMajor + iStateMinor;


            //　---- 興奮レベル判定 ----------------------------------------------------

            //　興奮度の判定　（通常版用）
            int iCurrentExcite = maid.status.currentExcite;
            int iExciteLevelModded = 0;


            //　興奮度を興奮レベルに変換
            if (iCurrentExcite < cfg.iExciteLevelThreshold1)
            {
                iExciteLevel = 1;
            }
            else if (cfg.iExciteLevelThreshold1 <= iCurrentExcite && iCurrentExcite < cfg.iExciteLevelThreshold2)
            {
                iExciteLevel = 2;
            }
            else if (cfg.iExciteLevelThreshold2 <= iCurrentExcite && iCurrentExcite < cfg.iExciteLevelThreshold3)
            {
                iExciteLevel = 3;
            }
            else if (cfg.iExciteLevelThreshold3 <= iCurrentExcite)
            {
                iExciteLevel = 4;
            }

            //　ピストン速度の判定　（Chu-BLip版用）
            if (bChuBLip && maid)
            {
                string sPistonSpeed = "";

                // sPistonSpeed = maid.onahole.motion.playedPistonSeType.ToString();
                // System.Reflectionを使って文字列でメンバーを取得する（OH版以外ではMaidにonahole以下が存在しないので、上の様に記述するとコンパイルできない）
                try
                {
                    sPistonSpeed = Util.GetObject.ByString(maid, "onahole.motion.playedPistonSeType").ToString();
                    if (sPistonSpeed != null)
                    {
                        debugPrintConsole(sPistonSpeed);
                    }
                }
                catch (Exception ex)
                {
                    debugPrintConsole(ex.ToString());
                }

                ////Reflection使って取得する他の例

                ////var cm = GameMain.Instance.CharacterMgr と同等のコード
                //var cm = Util.GetObject.ByString("GameMain.m_objInstance.m_CharacterMgr") as CharacterMgr;

                ////maid.Param.status.first_name と同等のコード
                //var first_name = Util.GetObject.ByString(maid, "m_Param.status_.first_name") as string;


                ////特定の型のフィールド一覧は下記で出力できる（デバッグで眺めてもよい）
                //Type t = typeof(Maid);

                ////メンバを取得する
                //MemberInfo[] members = t.GetMembers(
                //    BindingFlags.Public | BindingFlags.NonPublic |
                //    BindingFlags.Instance | BindingFlags.Static |
                //    BindingFlags.DeclaredOnly);
                //foreach (MemberInfo m in members)
                //{
                //    //メンバの型と、名前を表示する
                //    Console.WriteLine("{0} - {1}", m.MemberType, m.Name);
                //}


                //　ピストン速度を興奮度レベルに置き換える
                if (sPistonSpeed == "低速")
                {
                    iExciteLevel = 2;
                }
                else if (sPistonSpeed == "中速")
                {
                    iExciteLevel = 3;
                }
                else if (sPistonSpeed == "高速")
                {
                    iExciteLevel = 4;
                }
                else
                {
                    // "停止" or NO_DATA
                    iExciteLevel = 1;
                }

                //　スピードが変わったら音声を改めて適用する
                if (iExciteLevel != iExciteLevelOld)
                {
                    bPistonSpeedChanged = true;
                    debugPrintConsole("piston speed changed.");
                }

                iExciteLevelOld = iExciteLevel;
            }

            //　手動変更値で補正した興奮レベルが1～4から外れる時は、手動変更分を抑える
            if (4 < (iExciteLevel + iExciteLevelMod)) iExciteLevelMod = 4 - iExciteLevel; // 手動変更値の頭打ち
            if ((iExciteLevel + iExciteLevelMod) < 1) iExciteLevelMod = 1 - iExciteLevel; //手動変更値の底打ち
            iExciteLevelModded = Math.Min(4, iExciteLevel + iExciteLevelMod);
            debugPrintConsole("iExciteLevel, Mod, Modded = " + iExciteLevel + " , " + iExciteLevelMod + " , " +
                              iExciteLevelModded);


            //　---- 喋らせる ----------------------------------------------------

            //　ループ音声の適用

            //　ループ音声が流れているときはバックアップする（オーバライドしたものは除く）
            //　夜伽前の会話シーンなど、ループ音声が拾えない時もあるので注意すること
            if (cfg.bVoiceOverrideEnabled)
            {
                //　ループ音声のバックアップ（通常版）
                if (!bChuBLip && !bIsVoiceOverriding && maid.AudioMan.audiosource.loop &&
                    maid.AudioMan.audiosource.isPlaying)
                {
                    bOverrideInterrupted = false;
                    sLoopVoiceBackup = maid.AudioMan.FileName;
                    debugPrintConsole("voice backup done. " + sLoopVoiceBackup);
                }

                //　一回再生音声のバックアップ（ChuBLip版）
                if (bChuBLip && !bIsVoiceOverriding && !bIsEditScene)
                {
                    bOverrideInterrupted = false;
                    sLoopVoiceBackup = maid.AudioMan.FileName;
                    debugPrintConsole("voice backup done. " + sLoopVoiceBackup);
                }


                //　音声オーバライド判定開始ここから
                bool bAllowVoiceOverride = false;

                //　「とても近い」
                if (iStateMajor == 30)
                {
                    // 割り込みされた後、バックアップが拾えてない状況ではない
                    if (!bOverrideInterrupted)
                    {
                        //オーバーライドまたは音声表情変更のチェック
                        if (!maid.AudioMan.audiosource.isPlaying)
                        {
                            //音声が止まっていたらオーバーライドまたは音声入れ替え実行
                            bAllowVoiceOverride = true;
                        }
                        else if (maid.AudioMan.audiosource.loop)
                        {
                            //ループ音声再生中ならオーバーライド実行
                            bAllowVoiceOverride = true;
                        }
                        else if (bIsVoiceOverriding)
                        {
                            //オーバーライド中
                            if (playingMaid != maid || playingTargetPart != sTargetMaidPart || iStateHoldTime == 0)
                            {
                                //メイドか部位が変わったら音声入れ替え または タイマーが0なら表情変更
                                bAllowVoiceOverride = true;
                            }
                        }

                        //　ChuBLip版では、ループ音声が夜伽で使われないので、上の条件によらず許可する
                        if (bChuBLip)
                        {
                            if (!bIsVoiceOverriding || iStateHoldTime == 0)
                            {
                                bAllowVoiceOverride = true;
                            }

                            //　ChuBLip版では、ピストン速度に変化があった時に、強制的に新しい音声を再生しなおす
                            if (bPistonSpeedChanged)
                            {
                                bPistonSpeedChanged = false;
                                bAllowVoiceOverride = true;
                                iStateHoldTime = 0;
                            }
                        }
                    }
                }

                //　上記でオーバーライドが許可されたら実際に再生する
                if (bAllowVoiceOverride)
                {
                    bIsVoiceOverriding = true;

                    string[][] sLoopVoice30;

                    string uniqueName = maid.status.personal.uniqueName;
                    int customId = -1;

                    //メイドの名前からカスタムIDを取得
                    for (int i = 0; i < cfg.sLoopVoice30CustomMaidName.Length; i++)
                    {
                        if (cfg.sLoopVoice30CustomMaidName[i] == maid.status.lastName + maid.status.firstName)
                        {
                            customId = i;
                            break;
                        }
                    }

                    //メイドと部位に対応した音声取得
                    if (sTargetMaidPart == "maidHead")
                    {
                        sLoopVoice30 = getLoopVoice(uniqueName, "Excite", customId);
                        //ボイスがなければ無垢を設定
                        if (sLoopVoice30 == null || sLoopVoice30.Length < iExciteLevelModded ||
                            sLoopVoice30[iExciteLevelModded - 1].Length == 0) sLoopVoice30 = cfg.sLoopVoice30MukuExcite;
                    }
                    else if (sTargetMaidPart == "maidKokan")
                    {
                        sLoopVoice30 = getLoopVoice(uniqueName, "Kokan", customId);
                        //ボイスがなければMuneを設定
                        if (sLoopVoice30 == null || sLoopVoice30.Length < iExciteLevelModded ||
                            sLoopVoice30[iExciteLevelModded - 1].Length == 0)
                            sLoopVoice30 = getLoopVoice(uniqueName, "Mune", customId);
                        //ボイスがなければ無垢を設定
                        if (sLoopVoice30 == null || sLoopVoice30.Length < iExciteLevelModded ||
                            sLoopVoice30[iExciteLevelModded - 1].Length == 0) sLoopVoice30 = cfg.sLoopVoice30MukuMune;
                    }
                    else
                    {
                        sLoopVoice30 = getLoopVoice(uniqueName, "Mune", customId);
                        //ボイスがなければ無垢を設定
                        if (sLoopVoice30 == null || sLoopVoice30.Length < iExciteLevelModded ||
                            sLoopVoice30[iExciteLevelModded - 1].Length == 0) sLoopVoice30 = cfg.sLoopVoice30MukuMune;
                    }

                    //ボイスファイル 可変長に対応
                    string sVoiceFileName =
                        sLoopVoice30[iExciteLevelModded - 1][
                            UnityEngine.Random.Range(0, sLoopVoice30[iExciteLevelModded - 1].Length)];

                    //Console.WriteLine("playingMaid="+playingMaid.status.personal.uniqueName+" , playingTargetPart="+playingTargetPart+" , playingExciteLevel="+playingExciteLevel);

                    //音声を再生 キスのループ音声終了時 or 音声が変わっている or 対象メイドか部位かレベルが変わった
                    if (fLoopVoiceEndTime < Time.time || sLoopVoiceOverriding != maid.AudioMan.FileName
                                                      || playingMaid != maid || playingTargetPart != sTargetMaidPart ||
                                                      playingExciteLevel != iExciteLevelModded)
                    {
                        maid.AudioMan.LoadPlay(sVoiceFileName, 0f, false, true); //ループで再生
                        Console.WriteLine("[KissYourMaid] sLoopVoice30=" + sVoiceFileName);
                        //ループ音声の終了時間
                        fLoopVoiceEndTime = Time.time + maid.AudioMan.audiosource.clip.length;
                        //再生を始めたファイル名、メイド、部位、レベルを記憶
                        sLoopVoiceOverriding = maid.AudioMan.FileName;
                        playingMaid = maid;
                        playingTargetPart = sTargetMaidPart;
                        playingExciteLevel = iExciteLevelModded;
                    }
                }


                //　音声の割り込まれ判定
                //　音声オーバライド状態において、一回再生音声に割り込まれたら、
                //　オーバライド状態を一度解除し、バックアップ音声を拾い直す（キスしながらイッてぐったりしたメイドさんに、イク前の発情ボイスを復元してしまうといった事故を防ぐ）

                //　音声オーバライド状態で…
                if (bIsVoiceOverriding)
                {
                    //　音を切り替えるタイミングではないのに…
                    if ((iStateMajor == 30 || iStateHoldTime != 0)
                        || (iStateMajor == 20 || iStateHoldTime == iStateAltTime1)
                        || (iStateMajor == 40 || iStateHoldTime == iStateAltTime1))
                    {
                        //　再生中の音声が、オーバライドした音声と一致しないなら、割り込まれ状態と判断する
                        if (maid.AudioMan.FileName != sLoopVoiceOverriding)
                        {
                            bOverrideInterrupted = true;
                            bIsVoiceOverriding = false;
                            sLoopVoiceBackup = "";
                            debugPrintConsole("override interrupted." + sLoopVoiceBackup);
                        }
                    }
                }


                //　割り込まれたのが夜伽前の会話の場合、次の音声が無く、ループ音声を回収できないので割り込まれ状態を解除する
                if (bOverrideInterrupted && !maid.AudioMan.audiosource.loop && !maid.AudioMan.audiosource.isPlaying)
                {
                    bOverrideInterrupted = false;
                }

                //　音声オーバライドの停止と復元
                if (iStateMajor != 30 && bIsVoiceOverriding)
                {
                    //　オーバライド状態を解除
                    bIsVoiceOverriding = false;
                    bOverrideInterrupted = false;

                    fLoopVoiceEndTime = 0;
                    playingMaid = null;
                    playingTargetPart = "";
                    playingExciteLevel = 0;

                    if (!bChuBLip)
                    {
                        //　復元もしくは停止（170206追記 エディットor撮影ではループ音声を拾えていると延々流してしまうので停止する）
                        if (sLoopVoiceBackup != "" && !bIsEditScene)
                        {
                            maid.AudioMan.LoadPlay(sLoopVoiceBackup, 0f, false, true);
                            debugPrintConsole("voice restore done. " + sLoopVoiceBackup);
                        }
                        else
                        {
                            maid.AudioMan.Stop();
                            debugPrintConsole("voice stop done. " + sLoopVoiceBackup);
                        }
                    }
                    else
                    {
                        //　Chu-B Lip版は復元をせず流しっぱなし（現状、音声セリフ付きの音を復元してしまい不自然になることがある）
                        //　ただしエディットor撮影のときは止める
                        if (bIsEditScene)
                        {
                            maid.AudioMan.Stop();
                            debugPrintConsole("voice stop done. " + sLoopVoiceBackup);
                        }
                    }
                }
            }


            //　---- 表情を変える ----------------------------------------------------

            //　フェイスアニメの適用
            bool bAllowChangeFaceAnime = false;

            //　遷移直後かカウンタリセット時のタイミングで適用
            if ((iStateHoldTime == 0)
                || (iStateMajor == 20 && iStateHoldTime == iStateAltTime1)
                || (iStateMajor == 40 && iStateHoldTime == iStateAltTime1))
            {
                bAllowChangeFaceAnime = true;
            }

            int iRandomFace = 0;
            if (bAllowChangeFaceAnime)
            {
                string sFaceAnimeName = "";

                if (!cfg.bFukigenEnabled)
                {
                    //不機嫌モードでない
                    if (iState == 20)
                    {
                        iRandomFace = UnityEngine.Random.Range(0, cfg.sFaceAnime20.Length);
                        sFaceAnimeName = cfg.sFaceAnime20[iRandomFace];
                    }

                    if (iState == 21)
                    {
                        iRandomFace = UnityEngine.Random.Range(0, cfg.sFaceAnime21.Length);
                        sFaceAnimeName = cfg.sFaceAnime21[iRandomFace];
                    }

                    if (iState == 40)
                    {
                        iRandomFace = UnityEngine.Random.Range(0, cfg.sFaceAnime40.Length);
                        sFaceAnimeName = cfg.sFaceAnime40[iRandomFace];
                    }

                    if (iState == 41)
                    {
                        iRandomFace = UnityEngine.Random.Range(0, cfg.sFaceAnime41.Length);
                        sFaceAnimeName = cfg.sFaceAnime41[iRandomFace];
                    }

                    if (iState == 30)
                    {
                        if (sTargetMaidPart == "maidHead")
                        {
                            //口へのキスの場合
                            iRandomFace = UnityEngine.Random.Range(0,
                                cfg.sFaceAnime30Excite[iExciteLevelModded - 1].Length);
                            sFaceAnimeName = cfg.sFaceAnime30Excite[iExciteLevelModded - 1][iRandomFace];
                        }
                        else
                        {
                            //口以外へのキスの場合
                            iRandomFace = UnityEngine.Random.Range(0,
                                cfg.sFaceAnime30ExciteMune[iExciteLevelModded - 1].Length);
                            sFaceAnimeName = cfg.sFaceAnime30ExciteMune[iExciteLevelModded - 1][iRandomFace];
                        }
                    }
                }
                else
                {
                    //不機嫌モードである
                    if (iState == 20)
                    {
                        iRandomFace = UnityEngine.Random.Range(0, cfg.sFaceAnime20Fukigen.Length);
                        sFaceAnimeName = cfg.sFaceAnime20Fukigen[iRandomFace];
                    }

                    if (iState == 21)
                    {
                        iRandomFace = UnityEngine.Random.Range(0, cfg.sFaceAnime21Fukigen.Length);
                        sFaceAnimeName = cfg.sFaceAnime21Fukigen[iRandomFace];
                    }

                    if (iState == 40)
                    {
                        iRandomFace = UnityEngine.Random.Range(0, cfg.sFaceAnime40Fukigen.Length);
                        sFaceAnimeName = cfg.sFaceAnime40Fukigen[iRandomFace];
                    }

                    if (iState == 41)
                    {
                        iRandomFace = UnityEngine.Random.Range(0, cfg.sFaceAnime41Fukigen.Length);
                        sFaceAnimeName = cfg.sFaceAnime41Fukigen[iRandomFace];
                    }

                    if (iState == 30)
                    {
                        if (sTargetMaidPart == "maidHead")
                        {
                            //口へのキスの場合
                            iRandomFace = UnityEngine.Random.Range(0,
                                cfg.sFaceAnime30FukigenExcite[iExciteLevelModded - 1].Length);
                            sFaceAnimeName = cfg.sFaceAnime30FukigenExcite[iExciteLevelModded - 1][iRandomFace];
                        }
                        else
                        {
                            //口以外へのキスの場合
                            iRandomFace = UnityEngine.Random.Range(0,
                                cfg.sFaceAnime30FukigenExciteMune[iExciteLevelModded - 1].Length);
                            sFaceAnimeName = cfg.sFaceAnime30FukigenExciteMune[iExciteLevelModded - 1][iRandomFace];
                        }
                    }
                }

                //　""か"変更しない"でなければ、フェイスアニメを適用する
                if (sFaceAnimeName != "" && sFaceAnimeName != "変更しない")
                {
                    maid.FaceAnime(sFaceAnimeName, cfg.fAnimeFadeTime, 0);
                }
            }


            //　フェイスブレンドの適用（近い・とても近いのみ）
            //　ステートに応じたフェイスブレンドに上書きする。
            //　ただし、より強いものが適用されるなら、そちらを尊重して上書きしない

            if (iStateMajor != 10 && bAllowChangeFaceAnime)
            {
                string sFaceBlendCurrent = maid.FaceName3;
                debugPrintConsole(sFaceBlendCurrent);

                if (sFaceBlendCurrent == "") sFaceBlendCurrent = "頬０涙０"; // 背景選択時、スキル選択時は、"" が返ってきてエラーが出るため

                string sCurrentCheek = "";
                string sCurrentTears = "";
                int iCurrentCheek = 0;
                int iCurrentTears = 0;
                bool bCurrentYodare = false;

                string sChangeCheek = "";
                string sChangeTears = "";
                int iChangeCheek = 0;
                int iChangeTears = 0;
                string sChangeYodare = "";
                string sChangeBlend = "";

                int iOverrideCheek = 0;
                int iOverrideTears = 0;
                bool bOverrideYodare = false;


                //　どのステート対しても、同じフェイスブレンドを適用する（ステート毎に変えるつもりだったが、実際やってみたらいまいちだった）
                iOverrideCheek = 2; //"頬２"
                iOverrideTears = 0; //"涙０"

                //　顔目追従
                if (iState != 40)
                {
                    if (sTargetMaidPart == "maidHead")
                    {
                        if (!cfg.bFukigenEnabled)
                        {
                            //　不機嫌モードでない
                            if (UnityEngine.Random.Range(0, 100) < cfg.iPercentLookAway)
                            {
                                if (cfg.bLookAwayOnlyEye)
                                {
                                    maid.EyeToCamera(Maid.EyeMoveType.目だけそらす, cfg.fAnimeFadeTime);
                                }
                                else
                                {
                                    maid.EyeToCamera(Maid.EyeMoveType.顔をそらす, cfg.fAnimeFadeTime);
                                }
                            }
                            else
                            {
                                maid.EyeToCamera(Maid.EyeMoveType.目と顔を向ける, cfg.fAnimeFadeTime);
                            }
                        }
                        else
                        {
                            //　不機嫌モード
                            if (UnityEngine.Random.Range(0, 100) < cfg.iPercentLookAwayFukigen)
                            {
                                if (cfg.bLookAwayOnlyEyeFukigen)
                                {
                                    maid.EyeToCamera(Maid.EyeMoveType.目だけそらす, cfg.fAnimeFadeTime);
                                }
                                else
                                {
                                    maid.EyeToCamera(Maid.EyeMoveType.顔をそらす, cfg.fAnimeFadeTime);
                                }
                            }
                            else
                            {
                                maid.EyeToCamera(Maid.EyeMoveType.目と顔を向ける, cfg.fAnimeFadeTime);
                            }
                        }
                    }
                    else
                    {
                        //　顔以外のところは半々で顔を向ける
                        if (cfg.iPercentLookAwayOtherParts > 0 &&
                            UnityEngine.Random.Range(0, 100) < cfg.iPercentLookAwayOtherParts)
                        {
                            if (cfg.bLookAwayOnlyEyeOtherParts)
                            {
                                maid.EyeToCamera(Maid.EyeMoveType.目だけそらす, cfg.fAnimeFadeTime);
                            }
                            else
                            {
                                maid.EyeToCamera(Maid.EyeMoveType.顔をそらす, cfg.fAnimeFadeTime);
                            }
                        }
                        else
                        {
                            maid.EyeToCamera(Maid.EyeMoveType.目と顔を向ける, cfg.fAnimeFadeTime);
                        }
                    }

                    // Chu-B Lipの夜伽は、何故か「目だけそらす」と「顔をそらす」が効かないので、「目と顔を向ける」にする
                    if (bChuBLip && bIsYotogiScene)
                    {
                        maid.EyeToCamera(Maid.EyeMoveType.目と顔を向ける, cfg.fAnimeFadeTime);
                    }
                }


                //　よだれ（興奮レベルが一定以上の時にだけよだれをつける）
                bOverrideYodare = cfg.iYodareAppearLevel != 0 && iExciteLevelModded >= cfg.iYodareAppearLevel;

                //　元々のフェイスブレンドと比較する
                sCurrentCheek = sFaceBlendCurrent.Substring(0, 2);
                if (sCurrentCheek == "頬０") iCurrentCheek = 0;
                if (sCurrentCheek == "頬１") iCurrentCheek = 1;
                if (sCurrentCheek == "頬２") iCurrentCheek = 2;
                if (sCurrentCheek == "頬３") iCurrentCheek = 3;
                iChangeCheek = iCurrentCheek;
                if (iOverrideCheek > iChangeCheek) iChangeCheek = iOverrideCheek;
                if (iChangeCheek == 0) sChangeCheek = "頬０";
                if (iChangeCheek == 1) sChangeCheek = "頬１";
                if (iChangeCheek == 2) sChangeCheek = "頬２";
                if (iChangeCheek == 3) sChangeCheek = "頬３";

                sCurrentTears = sFaceBlendCurrent.Substring(2, 2);
                if (sCurrentTears == "涙０") iCurrentTears = 0;
                if (sCurrentTears == "涙１") iCurrentTears = 1;
                if (sCurrentTears == "涙２") iCurrentTears = 2;
                if (sCurrentTears == "涙３") iCurrentTears = 3;
                iChangeTears = iCurrentTears;
                if (iOverrideTears > iChangeTears) iChangeTears = iOverrideTears;
                if (iChangeTears == 0) sChangeTears = "涙０";
                if (iChangeTears == 1) sChangeTears = "涙１";
                if (iChangeTears == 2) sChangeTears = "涙２";
                if (iChangeTears == 3) sChangeTears = "涙３";

                if (sFaceBlendCurrent.Substring(3) == "よだれ") bCurrentYodare = true;
                if (bCurrentYodare || bOverrideYodare) sChangeYodare = "よだれ";

                sChangeBlend = sChangeCheek + sChangeTears + sChangeYodare;
                maid.FaceBlend(sChangeBlend);

                debugPrintConsole("FaceBlendLevel=" + iChangeCheek + " , " + iChangeTears + " , " + sChangeYodare);
            }


            //　「離れている」に戻る時に、フェイスアニメ・フェイスブレンドを復元する（現在フェイスアニメのみ復元）
            if (iStateMajor == 10 && iStateMajorOld != 10) restoreFace();

            //　距離ステートの過去値を更新
            iStateMajorOld = iStateMajor;

            //　ターゲット部位の過去値を更新
            sTargetMaidPartOld = sTargetMaidPart;

            //　距離ステート保持時間を加算する（30でカンスト）
            if (iStateHoldTime++ > 30) iStateHoldTime = 30;


            debugPrintConsole("Level=" + iSceneLevel + " bIsYotogi=" + bIsYotogiScene +
                              " iState=" + iState + " fDistanceToTarget=" + fDistanceToTarget);
        }


        //　フェイスアニメ・フェイスブレンドのバックアップ
        private void backupFace()
        {
            sFaceAnimeBackup = maid.ActiveFace;
            sFaceBlendBackup = maid.FaceName3;
            //　頭の向き追従状態も保存したいが何処に保存されているか分からない

            debugPrintConsole("face backup done. " + sFaceAnimeBackup + " , " + sFaceBlendBackup);
        }


        //　フェイスアニメ・フェイスブレンドの復元
        private void restoreFace()
        {
            //　フェイスアニメの復元
            if (sFaceAnimeBackup != "")
            {
                maid.FaceAnime(sFaceAnimeBackup, cfg.fAnimeFadeTime * 2, 0);
            }

            //　フェイスブレンドの復元
            //maid.FaceBlend(sFaceBlendBackup);
            //　近づいた時の状況を復元するので、離れた時にスキルが変わっていると表情が食い違って不自然になることがあるが、現状いい方法がないので保留…
            //　フェイスブレンドも復元するつもりだったが、突然素に戻ったようになってしまうことが有ったので、戻さないようにした

            //　撮影モードの時は「オリジナル」に戻さないと表情パネルの変更が反映されないので戻す 161219
            if (GameMain.Instance.GetNowSceneName() == "ScenePhotoMode")
            {
                maid.FaceBlend("オリジナル");
            }

            //　顔目追従の復元
            //  できればキス前の状態を復元したいが、どの変数に保存されているか分からないので保留…
            //　170206追記　だいたい「顔と目を向ける」の状態のままだったが、不機嫌モードでの顔目そらしから戻せるオプションに
            if (cfg.bOnRestoreLookAtMe)
            {
                maid.EyeToCamera(Maid.EyeMoveType.目と顔を向ける, cfg.fAnimeFadeTime);
            }

            //　バックアップ内容を空にする
            sFaceAnimeBackup = "";
            sFaceBlendBackup = "";

            debugPrintConsole("face restore done. " + sFaceAnimeBackup + " , " + sFaceBlendBackup);
        }


        //　夜伽シーンにいるかをチェック
        private void checkYotogiScene()
        {
            string sceneName = GameMain.Instance.GetNowSceneName();
            bIsYotogiScene = sceneName.StartsWith("SceneYotogi") || sceneName.StartsWith("SceneVR");
        }


        //　エディットor撮影シーンにいるかをチェック
        private void checkEditScene()
        {
            bIsEditScene = (GameMain.Instance.GetNowSceneName() == "SceneEdit" ||
                            GameMain.Instance.GetNowSceneName() == "ScenePhotoMode");
        }


        //フェラしてるかチェックし、フェラし始めた時に顔追従を解除する
        private void checkBlowjobing()
        {
            if (maid && bIsYotogiScene)
            {
                //メイドさんのモーションファイル名に含まれる文字列で判別させる
                sLastAnimeFileName = maid.body0.LastAnimeFN;

                if (sLastAnimeFileName != null)
                {
                    bIsBlowjobing = false;

                    if (sLastAnimeFileName.Contains("fera")) bIsBlowjobing = true; //フェラ
                    if (sLastAnimeFileName.Contains("sixnine")) bIsBlowjobing = true; //シックスナイン
                    if (sLastAnimeFileName.Contains("_ir_")) bIsBlowjobing = true; //イラマ
                    if (sLastAnimeFileName.Contains("_kuti")) bIsBlowjobing = true; //乱交３Ｐ
                    if (sLastAnimeFileName.Contains("housi")) bIsBlowjobing = true; //乱交奉仕

                    if (sLastAnimeFileName.Contains("taiki"))
                    {
                        bIsBlowjobing = false; //待機中は含めない
                        if (sLastAnimeFileName.Contains("ir_in_taiki")) bIsBlowjobing = true; //咥え始めはフェラに含める
                        if (sLastAnimeFileName.Contains("dt_in_taiki")) bIsBlowjobing = true; //咥え始めはフェラに含める
                        if (sLastAnimeFileName.Contains("kuti_in_taiki")) bIsBlowjobing = true; //咥え始めはフェラに含める
                        if (sLastAnimeFileName.Contains("kutia_in_taiki")) bIsBlowjobing = true; //咥え始めはフェラに含める
                    }

                    if (sLastAnimeFileName.Contains("shaseigo")) bIsBlowjobing = false; //射精後は含めない
                    if (sLastAnimeFileName.Contains("surituke")) bIsBlowjobing = false; //乱交３Ｐ擦り付け時は咥えないのでは含めない


                    //フェラしはじめた時は顔追従を解除する（目追従は残す）
                    if (bIsBlowjobing)
                    {
                        if (sLastAnimeFileName != sLastAnimeFileNameOld)
                        {
                            if (cfg.bDontLookAtMeInFellatio)
                            {
                                maid.EyeToCamera((Maid.EyeMoveType)0, 0f);
                            }
                            else
                            {
                                maid.EyeToCamera((Maid.EyeMoveType)6, 0f);
                            }

                            debugPrintConsole("EyeMoveType reset done. ");
                        }
                    }

                    sLastAnimeFileNameOld = sLastAnimeFileName;
                }
            }
        }

        private AnimationState GetCurrentAnimationState(Maid maid)
        {
            AnimationState state = null;

            // アニメーション取得
            Animation anime = maid.body0.GetAnimation();
            if (anime != null)
            {
                // アニメーション状態取得
                state = anime[maid.body0.LastAnimeFN];
            }

            return state;
        }

        //　デバッグ用コンソール出力メソッド
        [Conditional("DEBUG")]
        private void debugPrintConsole(string s)
        {
            Console.WriteLine(s);
        }


        // GearMenu用のアイコン（有効時）
        internal static class KissEnableIcon
        {
            public static byte[] Png
            {
                get
                {
                    if (png == null)
                    {
                        // 32x32 ピクセルの PNG データを Base64 エンコードしたもの
                        png = Convert.FromBase64String(
                            "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"
                        );
                    }

                    return png;
                }
            }

            static byte[] png = null;
        }


        // GearMenu用のアイコン（無効時）
        internal static class KissDisableIcon
        {
            public static byte[] Png
            {
                get
                {
                    if (png == null)
                    {
                        // 32x32 ピクセルの PNG データを Base64 エンコードしたもの
                        png = Convert.FromBase64String(
                            "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"
                        );
                    }

                    return png;
                }
            }

            static byte[] png = null;
        }
    }
}


//　以下、cm3d2.reflectiontest.plugin.cs より拝借（ありがとうございました）
namespace Util
{
    public static class GetObject
    {
        /// <summary>
        /// static変数を取得する
        /// </summary>
        /// <example>
        /// <code>
        /// var cm = GetObject.ByString("GameMain.m_objInstance.m_CharacterMgr") as CharacterMgr;
        /// Console.WriteLine("cm = {0}", cm);
        /// </code>
        /// </example>
        public static object ByString(string longFieldName)
        {
            string[] ss = longFieldName.Split('.');
            return ByString(TypeByString(ss[0]), string.Join(".", ss, 1, ss.Length - 1));
        }

        /// <summary>
        /// 型情報とフィールド名を元にstatic変数を取得する
        /// </summary>
        /// <example>
        /// 「var cm = GameMain.Instance.CharacterMgr」と同等のコード例
        /// <code>
        /// var cm = GetObject.ByString(typeof(GameMain), "m_objInstance.m_CharacterMgr") as CharacterMgr;
        /// Console.WriteLine("cm = {0}", cm);
        /// </code>
        /// </example>
        public static object ByString(Type type, string longFieldName)
        {
            return ByString(type, null, longFieldName);
        }

        /// <summary>
        /// インスタンス変数を取得する
        /// </summary>
        /// <example>
        /// 「var first_name = maid.Param.status.first_name」と同等のコード例
        /// <code>
        /// Maid maid = GameMain.Instance.CharacterMgr.GetStockMaid(0);
        /// var first_name = GetObject.ByString(maid, "m_Param.status_.first_name") as string;
        /// Console.WriteLine("first_name = {0}", first_name);
        /// </code>
        /// </example>
        public static object ByString(object instance, string longFieldName)
        {
            return ByString(null, instance, longFieldName);
        }

        //
        public static object ByString(Type type, object instance, string longFieldName)
        {
            string[] fieldNames = longFieldName.Split('.');
            foreach (string fieldName in fieldNames)
            {
                if (instance != null)
                {
                    type = instance.GetType();
                }

                if (type == null)
                {
                    return null;
                }

                FieldInfo fi = type.GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.Static |
                    BindingFlags.Public | BindingFlags.NonPublic);
                if (fi == null)
                {
                    return null;
                }

                instance = fi.GetValue(instance);
            }

            return instance;
        }

        public static Type TypeByString(string typeName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}


//以下、下記より拝借（ありがとうございました）
//https://github.com/neguse11/cm3d2_plugins_okiba/blob/master/Lib/GearMenu.cs

namespace GearMenu
{
    /// <summary>
    /// 歯車メニューへのアイコン登録
    /// </summary>
    public static class Buttons
    {
        // 識別名の実体。同じ名前を保持すること。詳細は SetAndCallOnReposition を参照
        static string Name_ = "CM3D2.GearMenu.Buttons";

        // バージョン文字列の実体。改善、改造した場合は文字列の辞書順がより大きい値に更新すること
        static string Version_ = Name_ + " 0.0.2.0";

        /// <summary>
        /// 識別名
        /// </summary>
        public static string Name
        {
            get { return Name_; }
        }

        /// <summary>
        /// バージョン文字列
        /// </summary>
        public static string Version
        {
            get { return Version_; }
        }

        /// <summary>
        /// 歯車メニューにボタンを追加
        /// </summary>
        /// <param name="plugin">ボタンを追加するプラグイン。アイコンへのマウスオーバー時に名前とバージョンが表示される</param>
        /// <param name="pngData">アイコン画像。null可(システムアイコン使用)。32x32ピクセルのPNGファイル</param>
        /// <param name="action">コールバック。null可(コールバック削除)。アイコンクリック時に呼び出されるコールバック</param>
        /// <returns>生成されたボタンのGameObject</returns>
        /// <example>
        /// ボタン追加例
        /// <code>
        /// public class MyPlugin : UnityInjector.PluginBase {
        ///     void Awake() {
        ///         GearMenu.Buttons.Add(this, null, GearMenuCallback);
        ///     }
        ///     void GearMenuCallback(GameObject goButton) {
        ///         Debug.LogWarning("GearMenuCallback呼び出し");
        ///     }
        /// }
        /// </code>
        /// </example>
        public static GameObject Add(UnityInjector.PluginBase plugin, byte[] pngData, Action<GameObject> action)
        {
            return Add(null, plugin, pngData, action);
        }

        /// <summary>
        /// 歯車メニューにボタンを追加
        /// </summary>
        /// <param name="name">ボタンオブジェクト名。null可</param>
        /// <param name="plugin">ボタンを追加するプラグイン。アイコンへのマウスオーバー時に名前とバージョンが表示される</param>
        /// <param name="pngData">アイコン画像。null可(システムアイコン使用)。32x32ピクセルのPNGファイル</param>
        /// <param name="action">コールバック。null可(コールバック削除)。アイコンクリック時に呼び出されるコールバック</param>
        /// <returns>生成されたボタンのGameObject</returns>
        /// <example>
        /// ボタン追加例
        /// <code>
        /// public class MyPlugin : UnityInjector.PluginBase {
        ///     void Awake() {
        ///         GearMenu.Buttons.Add(GetType().Name, this, null, GearMenuCallback);
        ///     }
        ///     void GearMenuCallback(GameObject goButton) {
        ///         Debug.LogWarning("GearMenuCallback呼び出し");
        ///     }
        /// }
        /// </code>
        /// </example>
        public static GameObject Add(string name, UnityInjector.PluginBase plugin, byte[] pngData,
            Action<GameObject> action)
        {
            var pluginNameAttr =
                Attribute.GetCustomAttribute(plugin.GetType(), typeof(PluginNameAttribute)) as PluginNameAttribute;
            var pluginVersionAttr =
                Attribute.GetCustomAttribute(plugin.GetType(),
                    typeof(PluginVersionAttribute)) as PluginVersionAttribute;
            string pluginName = (pluginNameAttr == null) ? plugin.Name : pluginNameAttr.Name;
            string pluginVersion = (pluginVersionAttr == null) ? string.Empty : pluginVersionAttr.Version;
            string label = string.Format("{0} {1}", pluginName, pluginVersion);
            return Add(name, label, pngData, action);
        }

        /// <summary>
        /// 歯車メニューにボタンを追加
        /// </summary>
        /// <param name="label">ツールチップテキスト。null可(ツールチップ非表示)。アイコンへのマウスオーバー時に表示される</param>
        /// <param name="pngData">アイコン画像。null可(システムアイコン使用)。32x32ピクセルのPNGファイル</param>
        /// <param name="action">コールバック。null可(コールバック削除)。アイコンクリック時に呼び出されるコールバック</param>
        /// <returns>生成されたボタンのGameObject</returns>
        /// <example>
        /// ボタン追加例
        /// <code>
        /// public class MyPlugin : UnityInjector.PluginBase {
        ///     void Awake() {
        ///         GearMenu.Buttons.Add("テスト", null, GearMenuCallback);
        ///     }
        ///     void GearMenuCallback(GameObject goButton) {
        ///         Debug.LogWarning("GearMenuCallback呼び出し");
        ///     }
        /// }
        /// </code>
        /// </example>
        public static GameObject Add(string label, byte[] pngData, Action<GameObject> action)
        {
            return Add(null, label, pngData, action);
        }

        /// <summary>
        /// 歯車メニューにボタンを追加
        /// </summary>
        /// <param name="name">ボタンオブジェクト名。null可</param>
        /// <param name="label">ツールチップテキスト。null可(ツールチップ非表示)。アイコンへのマウスオーバー時に表示される</param>
        /// <param name="pngData">アイコン画像。null可(システムアイコン使用)。32x32ピクセルのPNGファイル</param>
        /// <param name="action">コールバック。null可(コールバック削除)。アイコンクリック時に呼び出されるコールバック</param>
        /// <returns>生成されたボタンのGameObject</returns>
        /// <example>
        /// ボタン追加例
        /// <code>
        /// public class MyPlugin : UnityInjector.PluginBase {
        ///     void Awake() {
        ///         GearMenu.Buttons.Add(GetType().Name, "テスト", null, GearMenuCallback);
        ///     }
        ///     void GearMenuCallback(GameObject goButton) {
        ///         Debug.LogWarning("GearMenuCallback呼び出し");
        ///     }
        /// }
        /// </code>
        /// </example>
        public static GameObject Add(string name, string label, byte[] pngData, Action<GameObject> action)
        {
            GameObject goButton = null;

            // 既に存在する場合は削除して続行
            if (Contains(name))
            {
                Remove(name);
            }

            if (action == null)
            {
                return goButton;
            }

            try
            {
                // ギアメニューの子として、コンフィグパネル呼び出しボタンを複製
                goButton = NGUITools.AddChild(Grid, UTY.GetChildObject(Grid, "Config", true));

                // 名前を設定
                if (name != null)
                {
                    goButton.name = name;
                }

                // イベントハンドラ設定（同時に、元から持っていたハンドラは削除）
                EventDelegate.Set(goButton.GetComponent<UIButton>().onClick, () => { action(goButton); });

                // ポップアップテキストを追加
                {
                    UIEventTrigger t = goButton.GetComponent<UIEventTrigger>();
                    EventDelegate.Add(t.onHoverOut, () => { SysShortcut.VisibleExplanation(null, false); });
                    EventDelegate.Add(t.onDragStart, () => { SysShortcut.VisibleExplanation(null, false); });
                    SetText(goButton, label);
                }

                // PNG イメージを設定
                {
                    if (pngData == null)
                    {
                        pngData = DefaultIcon.Png;
                    }

                    // 本当はスプライトを削除したいが、削除するとパネルのα値とのTween同期が動作しない
                    // (動作させる方法が分からない) ので、スプライトを描画しないように設定する
                    // もともと持っていたスプライトを削除する場合は以下のコードを使うと良い
                    //     NGUITools.Destroy(goButton.GetComponent<UISprite>());
                    UISprite us = goButton.GetComponent<UISprite>();
                    us.type = UIBasicSprite.Type.Filled;
                    us.fillAmount = 0.0f;

                    // テクスチャを生成
                    var tex = new Texture2D(1, 1);
                    tex.LoadImage(pngData);

                    // 新しくテクスチャスプライトを追加
                    UITexture ut = NGUITools.AddWidget<UITexture>(goButton);
                    ut.material = new Material(ut.shader);
                    ut.material.mainTexture = tex;
                    ut.MakePixelPerfect();
                }

                // グリッド内のボタンを再配置
                Reposition();
            }
            catch
            {
                // 既にオブジェクトを作っていた場合は削除
                if (goButton != null)
                {
                    NGUITools.Destroy(goButton);
                    goButton = null;
                }

                throw;
            }

            return goButton;
        }

        /// <summary>
        /// 歯車メニューからボタンを削除
        /// </summary>
        /// <param name="name">ボタン名。Add()に与えた名前</param>
        public static void Remove(string name)
        {
            Remove(Find(name));
        }

        /// <summary>
        /// 歯車メニューからボタンを削除
        /// </summary>
        /// <param name="go">ボタン。Add()の戻り値</param>
        public static void Remove(GameObject go)
        {
            NGUITools.Destroy(go);
            Reposition();
        }

        /// <summary>
        /// 歯車メニュー内のボタンの存在を確認
        /// </summary>
        /// <param name="name">ボタン名。Add()に与えた名前</param>
        public static bool Contains(string name)
        {
            return Find(name) != null;
        }

        /// <summary>
        /// 歯車メニュー内のボタンの存在を確認
        /// </summary>
        /// <param name="go">ボタン。Add()の戻り値</param>
        public static bool Contains(GameObject go)
        {
            return Contains(go.name);
        }

        /// <summary>
        /// ボタンに枠をつける
        /// </summary>
        /// <param name="name">ボタン名。Add()に与えた名前</param>
        /// <param name="color">枠の色</param>
        public static void SetFrameColor(string name, Color color)
        {
            SetFrameColor(Find(name), color);
        }

        /// <summary>
        /// ボタンに枠をつける
        /// </summary>
        /// <param name="go">ボタン。Add()の戻り値</param>
        /// <param name="color">枠の色</param>
        public static void SetFrameColor(GameObject go, Color color)
        {
            var uiTexture = go.GetComponentInChildren<UITexture>();
            if (uiTexture == null)
            {
                return;
            }

            var tex = uiTexture.mainTexture as Texture2D;
            if (tex == null)
            {
                return;
            }

            for (int x = 1; x < tex.width - 1; x++)
            {
                tex.SetPixel(x, 0, color);
                tex.SetPixel(x, tex.height - 1, color);
            }

            for (int y = 1; y < tex.height - 1; y++)
            {
                tex.SetPixel(0, y, color);
                tex.SetPixel(tex.width - 1, y, color);
            }

            tex.Apply();
        }

        /// <summary>
        /// ボタンの枠を消す
        /// </summary>
        /// <param name="name">ボタン名。Add()に与えた名前</param>
        public static void ResetFrameColor(string name)
        {
            ResetFrameColor(Find(name));
        }

        /// <summary>
        /// ボタンの枠を消す
        /// </summary>
        /// <param name="go">ボタンのGameObject。Add()の戻り値</param>
        public static void ResetFrameColor(GameObject go)
        {
            SetFrameColor(go, DefaultFrameColor);
        }

        /// <summary>
        /// マウスオーバー時のテキスト指定
        /// </summary>
        /// <param name="name">ボタン名。Add()に与えた名前</param>
        /// <param name="label">マウスオーバー時のテキスト。null可</param>
        public static void SetText(string name, string label)
        {
            SetText(Find(name), label);
        }

        /// <summary>
        /// マウスオーバー時のテキスト指定
        /// </summary>
        /// <param name="go">ボタンのGameObject。Add()の戻り値</param>
        /// <param name="label">マウスオーバー時のテキスト。null可</param>
        public static void SetText(GameObject go, string label)
        {
            var t = go.GetComponent<UIEventTrigger>();
            t.onHoverOver.Clear();
            EventDelegate.Add(t.onHoverOver, () => { SysShortcut.VisibleExplanation(label, label != null); });
            var b = go.GetComponent<UIButton>();

            // 既にホバー中なら説明を変更する
            if (b.state == UIButtonColor.State.Hover)
            {
                SysShortcut.VisibleExplanation(label, label != null);
            }
        }

        // システムショートカット内のGameObjectを見つける
        static GameObject Find(string name)
        {
            Transform t = GridUI.GetChildList().FirstOrDefault(c => c.gameObject.name == name);
            return t == null ? null : t.gameObject;
        }

        // グリッド内のボタンを再配置
        static void Reposition()
        {
            // 必要なら UIGrid.onRepositionを設定、呼び出しを行う
            SetAndCallOnReposition(GridUI);

            // 次回の UIGrid.Update 処理時にグリッド内のボタン再配置が行われるようリクエスト
            GridUI.repositionNow = true;
        }

        // 必要に応じて UIGrid.onReposition を登録、呼び出す
        static void SetAndCallOnReposition(UIGrid uiGrid)
        {
            string targetVersion = GetOnRepositionVersion(uiGrid);

            // バージョン文字列が null の場合、知らないクラスが登録済みなのであきらめる
            if (targetVersion == null)
            {
                return;
            }

            // 何も登録されていないか、自分より古いバージョンだったら新しい onReposition を登録する
            if (targetVersion == string.Empty || string.Compare(targetVersion, Version, false) < 0)
            {
                uiGrid.onReposition = (new OnRepositionHandler(Version)).OnReposition;
            }

            // PreOnReposition を持つ場合はそれを呼び出す
            if (uiGrid.onReposition != null)
            {
                object target = uiGrid.onReposition.Target;
                if (target != null)
                {
                    Type type = target.GetType();
                    MethodInfo mi = type.GetMethod("PreOnReposition");
                    if (mi != null)
                    {
                        mi.Invoke(target, new object[] { });
                    }
                }
            }
        }

        // UIGrid.onReposition を保持するオブジェクトのバージョン文字列を得る
        //  null            知らないクラスもしくはバージョン文字列だった
        //  string.Empty    UIGrid.onRepositionが未登録だった
        //  その他          取得したバージョン文字列
        static string GetOnRepositionVersion(UIGrid uiGrid)
        {
            if (uiGrid.onReposition == null)
            {
                // 未登録だった
                return string.Empty;
            }

            object target = uiGrid.onReposition.Target;
            if (target == null)
            {
                // Delegate.Target が null ということは、
                // UIGrid.onReposition は static なメソッドなので、たぶん知らないクラス
                return null;
            }

            Type type = target.GetType();
            if (type == null)
            {
                // 型情報が取れないので、あきらめる
                return null;
            }

            FieldInfo fi = type.GetField("Version", BindingFlags.Instance | BindingFlags.Public);
            if (fi == null)
            {
                // public な Version メンバーを持っていないので、たぶん知らないクラス
                return null;
            }

            string targetVersion = fi.GetValue(target) as string;
            if (targetVersion == null || !targetVersion.StartsWith(Name))
            {
                // 知らないバージョン文字列だった
                return null;
            }

            return targetVersion;
        }

        public static SystemShortcut SysShortcut
        {
            get { return GameMain.Instance.SysShortcut; }
        }

        public static UIPanel SysShortcutPanel
        {
            get { return SysShortcut.GetComponent<UIPanel>(); }
        }

        public static UISprite SysShortcutExplanation
        {
            get
            {
                Type type = typeof(SystemShortcut);
                FieldInfo fi = type.GetField("m_spriteExplanation", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fi == null)
                {
                    return null;
                }

                return fi.GetValue(SysShortcut) as UISprite;
            }
        }

        public static GameObject Base
        {
            get { return SysShortcut.gameObject.transform.Find("Base").gameObject; }
        }

        public static UISprite BaseSprite
        {
            get { return Base.GetComponent<UISprite>(); }
        }

        public static GameObject Grid
        {
            get { return Base.gameObject.transform.Find("Grid").gameObject; }
        }

        public static UIGrid GridUI
        {
            get { return Grid.GetComponent<UIGrid>(); }
        }

        public static readonly Color DefaultFrameColor = new Color(1f, 1f, 1f, 0f);

        // UIGrid.onReposition処理用のクラス
        // Delegate.Targetの値を生かすために、static ではなくインスタンスとして生成
        class OnRepositionHandler
        {
            public string Version;

            public OnRepositionHandler(string version)
            {
                this.Version = version;
            }

            public void OnReposition()
            {
            }

            public void PreOnReposition()
            {
                var g = GridUI;
                var b = BaseSprite;

                // ratio : 画面横幅に対するボタン全体の横幅の比率。0.5 なら画面半分
                float ratio = 3.0f / 4.0f;
                float pixelSizeAdjustment = UIRoot.GetPixelSizeAdjustment(Base);

                // g.cellWidth  = 39;
                g.cellHeight = g.cellWidth;
                g.arrangement = UIGrid.Arrangement.CellSnap;
                g.sorting = UIGrid.Sorting.None;
                g.pivot = UIWidget.Pivot.TopRight;
                g.maxPerLine = (int)(Screen.width / (g.cellWidth / pixelSizeAdjustment) * ratio);

                var children = g.GetChildList();
                int itemCount = children.Count;
                int spriteItemX = Math.Min(g.maxPerLine, itemCount);
                int spriteItemY = Math.Max(1, (itemCount - 1) / g.maxPerLine + 1);
                int spriteWidthMargin = (int)(g.cellWidth * 3 / 2 + 8);
                int spriteHeightMargin = (int)(g.cellHeight / 2);
                float pivotOffsetY = spriteHeightMargin * 1.5f + 1f;

                b.pivot = UIWidget.Pivot.TopRight;
                b.width = (int)(spriteWidthMargin + g.cellWidth * spriteItemX);
                b.height = (int)(spriteHeightMargin + g.cellHeight * spriteItemY + 2f);

                // (946,502) はもとの Base の localPosition の値
                // 他のオブジェクトから値を取れないだろうか？
                Base.transform.localPosition = new Vector3(946.0f, 502.0f + pivotOffsetY, 0.0f);

                // ここでの、高さ(spriteItemY)に応じて横方向に補正する意味が分からない。
                // たぶん何かを誤解している
                Grid.transform.localPosition = new Vector3(
                    -2.0f + (-spriteItemX - 1 + spriteItemY - 1) * g.cellWidth,
                    -1.0f - pivotOffsetY,
                    0f);

                {
                    int a = 0;
                    string[] specialNames = GameMain.Instance.CMSystem.NetUse ? OnlineButtonNames : OfflineButtonNames;
                    foreach (Transform child in children)
                    {
                        int i = a++;

                        // システムが持っているオブジェクトの場合は特別に順番をつける
                        int si = Array.IndexOf(specialNames, child.gameObject.name);
                        if (si >= 0)
                        {
                            i = si;
                        }

                        float x = (-i % g.maxPerLine + spriteItemX - 1) * g.cellWidth;
                        float y = (i / g.maxPerLine) * g.cellHeight;
                        child.localPosition = new Vector3(x, -y, 0f);
                    }
                }

                // マウスオーバー時のテキストの位置を指定
                {
                    UISprite sse = SysShortcutExplanation;
                    Vector3 v = sse.gameObject.transform.localPosition;
                    v.y = Base.transform.localPosition.y - b.height - sse.height;
                    sse.gameObject.transform.localPosition = v;
                }
            }

            // オンライン時のボタンの並び順。インデクスの若い側が右になる
            static string[] OnlineButtonNames = new string[]
            {
                "Config", "Ss", "SsUi", "Shop", "ToTitle", "Info", "Exit"
            };

            // オフライン時のボタンの並び順。インデクスの若い側が右になる
            static string[] OfflineButtonNames = new string[]
            {
                "Config", "Ss", "SsUi", "ToTitle", "Info", "Exit"
            };
        }
    }

    // デフォルトアイコン
    internal static class DefaultIcon
    {
        public static byte[] Png
        {
            get
            {
                if (png == null)
                {
                    // 32x32 ピクセルの PNG データを Base64 エンコードしたもの
                    png = Convert.FromBase64String(
                        "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAIAAAD8GO2jAAAAA3NCSVQICAjb4U/g" +
                        "AAAACXBIWXMAABYlAAAWJQFJUiTwAAAA/0lEQVRIie2WPYqFMBRGb35QiARM4QZS" +
                        "uAX3X7sDkWwgRYSQgJLEKfLGh6+bZywG/JrbnZPLJfChfd/hzuBb6QBA89i2zTln" +
                        "jFmWZV1XAPjrZgghAKjrum1bIUTTNFVVvQXOOaXUNE0xxhDC9++llBDS972U8iTQ" +
                        "Ws/zPAyDlPJreo5SahxHzrkQAo4baK0B4Dr9gGTgW4Ax5pxfp+dwzjH+JefhvaeU" +
                        "lhJQSr33J0GMsRT9A3j7P3gEj+ARPIJHUFBACCnLPYAvAWPsSpn4SAiBMXYSpJSs" +
                        "taUE1tqU0knQdR0AKKWu0zMkAwEA5QZnjClevHIvegnuq47o37frH81sg91rI7H3" +
                        "AAAAAElFTkSuQmCC"
                    );
                }

                return png;
            }
        }

        static byte[] png = null;
    }
}