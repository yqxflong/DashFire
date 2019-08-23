using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public sealed class PublishSubscribeSystem
  {
    private class ReceiptInfo
    {
      public string name_;
      public Delegate delegate_;
      public ReceiptInfo() { }
      public ReceiptInfo(string n, Delegate d)
      {
        name_ = n;
        delegate_ = d;
      }
    }

    public object Subscribe(string ev_name, string group, MyAction subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    public object Subscribe<T0>(string ev_name, string group, MyAction<T0> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    public object Subscribe<T0, T1>(string ev_name, string group, MyAction<T0, T1> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    public object Subscribe<T0, T1, T2>(string ev_name, string group, MyAction<T0, T1, T2> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    public object Subscribe<T0, T1, T2, T3>(string ev_name, string group, MyAction<T0, T1, T2, T3> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    public object Subscribe<T0, T1, T2, T3, T4>(string ev_name, string group, MyAction<T0, T1, T2, T3, T4> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    public object Subscribe<T0, T1, T2, T3, T4, T5>(string ev_name, string group, MyAction<T0, T1, T2, T3, T4, T5> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    public object Subscribe<T0, T1, T2, T3, T4, T5, T6>(string ev_name, string group, MyAction<T0, T1, T2, T3, T4, T5, T6> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    public object Subscribe<T0, T1, T2, T3, T4, T5, T6, T7>(string ev_name, string group, MyAction<T0, T1, T2, T3, T4, T5, T6, T7> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    public object Subscribe<T0, T1, T2, T3, T4, T5, T6, T7, T8>(string ev_name, string group, MyAction<T0, T1, T2, T3, T4, T5, T6, T7, T8> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    public object Subscribe<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(string ev_name, string group, MyAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    public object Subscribe<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string ev_name, string group, MyAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> subscriber) { return AddSubscriber(ev_name, group, subscriber); }
    public object Subscribe<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string ev_name, string group, MyAction<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> subscriber) { return AddSubscriber(ev_name, group, subscriber); }

    public void Unsubscribe(object receipt)
    {
      ReceiptInfo r = receipt as ReceiptInfo;
      Delegate d;
      if (null != r && subscribers_.TryGetValue(r.name_, out d)) {
        subscribers_[r.name_] = Delegate.Remove(d, r.delegate_);
      }
    }

    public void Publish(string ev_name, string group, params object[] parameters)
    {
      Delegate d;
      if (subscribers_.TryGetValue(group + '#' + ev_name, out d))
        d.DynamicInvoke(parameters);
    }

    private object AddSubscriber(string ev_name, string group, Delegate d)
    {
      Delegate source;
      string key = group + '#' + ev_name;
      if (subscribers_.TryGetValue(key, out source))
        source = Delegate.Combine(source, d);
      else
        source = d;
      subscribers_[key] = source;
      return new ReceiptInfo(key, d);
    }

    private Dictionary<string, Delegate> subscribers_ = new Dictionary<string, Delegate>();
  }
}
