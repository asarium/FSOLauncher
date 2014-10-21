#region Usings

using FSOManagement.Annotations;

#endregion

namespace FSOManagement.Interfaces
{
    public interface IDataModel
    { }

    public interface IDataModel<TDataType> : IDataModel where TDataType : struct
    {
        void InitializeFromData(TDataType data);

        TDataType GetData();
    }
}
