namespace DashFire
{
  public class StrDictionary : IData
  {
    public int Id = 0;
    public string m_String = "";

    public bool CollectDataFromDBC(DBC_Row node)
    {
      Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
      m_String = DBCUtil.ExtractString(node, "String", "", true);
      return true;
    }
    public int GetId()
    {
      return Id;
    }
  }
  public class StrDictionaryProvider
  {
    public string Format(int id, params object[] args)
    {
      string ret;
      StrDictionary dict = GetDataById(id);
      if (null != dict && null != dict.m_String) {
        ret = string.Format(dict.m_String, args);
      } else {
        ret = "";
      }
      return ret;
    }

    public DataTemplateMgr<StrDictionary> StrDictionaryMgr
    {
      get { return m_StrDictionaryMgr; }
    }
    public StrDictionary GetDataById(int id)
    {
      return m_StrDictionaryMgr.GetDataById(id);
    }
    public int GetDataCount()
    {
      return m_StrDictionaryMgr.GetDataCount();
    }
    public void Load(string file, string root)
    {
      m_StrDictionaryMgr.CollectDataFromDBC(file, root);
    }

    private DataTemplateMgr<StrDictionary> m_StrDictionaryMgr = new DataTemplateMgr<StrDictionary>();

    public static StrDictionaryProvider Instance
    {
      get { return s_Instance; }
    }
    private static StrDictionaryProvider s_Instance = new StrDictionaryProvider();
  }
}
