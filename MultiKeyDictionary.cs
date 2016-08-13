using System.Collections.Generic;

public class MultiKeyDictionary<K,V> : Dictionary<K,V>
{
    public new V this[K key]
    {
        get { 
            return GetValue(key); 
        }
        set {  
            base[key] = value;
        }
    } 

    protected virtual V GetValue(K key)
    {
        foreach(K k in Keys){
            if(k.Equals(key))
            {
                return  base[k];
            }
        }
        return default(V);
    }
}