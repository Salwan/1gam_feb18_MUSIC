using System.Collections;
using System.Collections.Generic;

public class Pair<T, U>{
    private T m_first;
    private U m_second;

    public Pair(T _first, U _second) {
        set(_first, _second);
    }

    public void set(T _first, U _second)
    {
        m_first = _first;
        m_second = _second;
    }

    public T first {
        get { return m_first; }
        set { m_first = value;  }
    }

    public U second {
        get { return m_second; }
        set { m_second = value; }
    }
}
