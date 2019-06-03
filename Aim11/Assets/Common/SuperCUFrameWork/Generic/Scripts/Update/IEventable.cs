//===============================================================
// ファイル名：IEventable.cs
// 作成者    ：村上一真
// Updateを管理するためのインターフェース
//===============================================================
namespace SuperCU
{
    public interface IEventable
    {
        void UpdateGame();
        void LateUpdateGame();
        void FixedUpdateGame();
    }
}