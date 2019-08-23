using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocketClient
{
  class Program
  {
    static void Main(string[] args)
    {
      TestClient client = new TestClient();
      client.Run();
    }
  }
}
