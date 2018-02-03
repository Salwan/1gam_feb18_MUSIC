using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is a simple signal-slot implementation used by the game for event management
// C# doesn't support variadic generics in 3.5 (or at all it seems as the implementation is different)
// C# events are basically Signal-Slot (Publisher and Subscribers). But it's fun implementing my own.

// SIGNAL T1
public class Signal<T1> {
    private List<Action<T1>> m_slots;
    public Signal() {
        m_slots = new List<Action<T1>>();
    }

    // Connects to slot, returns false if slot is already connected
    public bool connect(Action<T1> slot) {
        if(!m_slots.Contains(slot)) {
            m_slots.Add(slot);
            return true;
        } else {
            return false;
        }
    }

    // Disconnects from slot, returns false if slot is not found
    public bool disconnect(Action<T1> slot) {
        return m_slots.Remove(slot);
    }

    // Disconnects all slots, returns how many
    public int disconnectAll() {
        int c = m_slots.Count;
        m_slots.Clear();
        return c;
    }

    // Emits signal to all connected slots
    public int emit(T1 arg1) {
        foreach(Action<T1> sh in m_slots) {
            sh(arg1);
        }
        return m_slots.Count;
    }

    // Operators: removed because they don't compile when used via returning a signal from function
    public static Signal<T1> operator +(Signal<T1> sig, Action<T1> slot) {
        sig.connect(slot);
        return sig;
    }
    public static Signal<T1> operator -(Signal<T1> sig, Action<T1> slot) {
        sig.disconnect(slot);
        return sig;
    }
}

// SIGNAL T1 T2
public class Signal<T1,T2> {
    private List<Action<T1, T2>> m_slots;
    public Signal() {
        m_slots = new List<Action<T1, T2>>();
    }

    // Connects to slot, returns false if slot is already connected
    public bool connect(Action<T1, T2> slot) {
        if(!m_slots.Contains(slot)) {
            m_slots.Add(slot);
            return true;
        } else {
            return false;
        }
    }

    // Disconnects from slot, returns false if slot is not found
    public bool disconnect(Action<T1, T2> slot) {
        return m_slots.Remove(slot);
    }

    // Disconnects all slots, returns how many
    public int disconnectAll() {
        int c = m_slots.Count;
        m_slots.Clear();
        return c;
    }

    // Emits signal to all connected slots
    public int emit(T1 arg1, T2 arg2) {
        foreach(Action<T1, T2> sh in m_slots) {
            sh(arg1, arg2);
        }
        return m_slots.Count;
    }
}

// SIGNAL T1 T2 T3
public class Signal<T1,T2,T3> {
    private List<Action<T1, T2, T3>> m_slots;
    public Signal() {
        m_slots = new List<Action<T1, T2, T3>>();
    }

    // Connects to slot, returns false if slot is already connected
    public bool connect(Action<T1, T2, T3> slot) {
        if(!m_slots.Contains(slot)) {
            m_slots.Add(slot);
            return true;
        } else {
            return false;
        }
    }

    // Disconnects from slot, returns false if slot is not found
    public bool disconnect(Action<T1, T2, T3> slot) {
        return m_slots.Remove(slot);
    }

    // Disconnects all slots, returns how many
    public int disconnectAll() {
        int c = m_slots.Count;
        m_slots.Clear();
        return c;
    }

    // Emits signal to all connected slots
    public int emit(T1 arg1, T2 arg2, T3 arg3) {
        foreach(Action<T1, T2, T3> sh in m_slots) {
            sh(arg1, arg2, arg3);
        }
        return m_slots.Count;
    }
}

// Just testing
class HandlerTest {
    public void testHandle1(int a) {
        Debug.Log("Test Handle 1 = " + a.ToString());
    }

    public void testHandle2(int a) {
        Debug.Log("Test Handle 2 = " + a.ToString());
    }

    public void testHandle3(int a) {
        Debug.Log("Test Handle 3 = " + a.ToString());
    }
}

class SignalTest {
    public static void TestSignal1() {
        HandlerTest ht = new HandlerTest();
        Signal<int> sig = new Signal<int>();
        sig.connect(ht.testHandle1);
        sig.connect(ht.testHandle2);
        sig.connect(ht.testHandle3);
        sig.emit(1337);
        sig.connect(ht.testHandle1);
        //sig.connect(ht.testHandle2);
        sig += ht.testHandle2;
        sig.emit(4499);
    }

    public void testHandleInner(int a) {
        Debug.Log("Test Handle Inner = " + a.ToString());
    }
}