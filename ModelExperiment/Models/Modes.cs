using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public abstract class Material
    {
     
    }

    public abstract class Process
    {
        public List<Equipment> EquipmentNeeded;
        public List<Material> MaterialNeeded;

 
    }
    public abstract class WorkCell
    {
        
 
    }
    public abstract class Equipment
    {
        public List<Equipment> OwnEquitments;
        public void AddEquitment(Equipment equip)
        {
            OwnEquitments.Add(equip);
 
        }
        virtual public bool RegiestEquitmentToWorkCell();
 
    }
    public interface Control
    {
 
    }

    

}
