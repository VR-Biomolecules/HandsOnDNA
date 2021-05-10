using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * Responsible for reading and parsing the coordinate datafiles for our molecules.
 * Returns info classes that describe atom positions and bond connections. 
 */
public class DataReader : MonoBehaviour
{
    public TextAsset phosphateDataFile;
    public TextAsset sugarDataFile;
    public TextAsset adenineDataFile;
    public TextAsset thymineDataFile;
    public TextAsset guanineDataFile;
    public TextAsset cytosineDataFile;
    public TextAsset allCoords;

    public static MoleculeInfo ReadCsvCoords(TextAsset file, bool isPdb)
    {
        List<AtomInfo> atoms = new List<AtomInfo>();
        List<BondInfo> bonds = new List<BondInfo>();
        bool inBonds = false;

        string data = file.text;
        
        foreach (string line in data.Split(new[]{"\n"}, StringSplitOptions.None))
        {
            if (!inBonds)
            {
                if (line.Trim().Equals("BONDS"))
                {
                    inBonds = true;
                    continue;
                }
                
                if (!line.StartsWith("#") && !line.Trim().Equals(""))
                {
                    if (!isPdb)
                    {
                        atoms.Add(new AtomInfo(line.Trim(), false));
                        // Debug.Log("New atom loaded: " + line.Trim());
                    }
                    else if (line.StartsWith("ATOM") || line.StartsWith("HETATM"))
                    {
                        
                        atoms.Add(new AtomInfo(line.Trim(), true));
                        // Debug.Log("New atom loaded: " + line.Trim());
                    }
                }
            }
            else
            {
                if (!line.StartsWith("#") && !line.Trim().Equals(""))
                {
                    // Debug.Log("New bond loaded: " + line.Trim());
                    bonds.Add(new BondInfo(line.Trim(), isPdb, atoms));
                }
            }
        }
        return new MoleculeInfo(file.name, atoms, bonds);
    }

    public MoleculeInfo GetAdenineInfo()
    {
        return ReadCsvCoords(adenineDataFile, false);
    }
    
    public MoleculeInfo GetThymineInfo()
    {
        return ReadCsvCoords(thymineDataFile, false);
    }
    
    public MoleculeInfo GetGuanineInfo()
    {
        return ReadCsvCoords(guanineDataFile, false);
    }
    
    public MoleculeInfo GetCytosineInfo()
    {
        return ReadCsvCoords(cytosineDataFile, false);
    }

    public MoleculeInfo GetPhosphateInfo()
    {
        return ReadCsvCoords(phosphateDataFile, false);
    }

    public MoleculeInfo GetSugarInfo()
    {
        return ReadCsvCoords(sugarDataFile, false);
    }

    // Quick method to help Sam with cartoon model
    public List<Vector3> GetPhosphorusCoordsFromPDB()
    {
        List<Vector3> returnList = new List<Vector3>();
        foreach (AtomInfo atom in ReadCsvCoords(allCoords, true).atoms)
        {
            if (atom.name.Equals("P"))
            {
                returnList.Add(atom.coords);
            }
        }
        return returnList;
    }

    public class AtomInfo
    {
        public int id;
        public string name;
        public Atom.AtomType atomType;
        public Vector3 coords;

        public AtomInfo(string text, bool isPdb)
        {
            if (!isPdb)
            {
                string[] values = text.Split(new[] {" ", "\t"}, StringSplitOptions.RemoveEmptyEntries);
                id = int.Parse(values[0].Trim());
                name = values[1].Trim();
                atomType = Atom.DetectAtom(name);
                coords = new Vector3(float.Parse(values[2]), float.Parse(values[3]), float.Parse(values[4]));
            }
            else
            {
                string[] values = text.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);

                // if (values[0].Equals("ATOM"))
                // {
                    id = int.Parse(values[1].Trim());
                    atomType = Atom.DetectAtom(values[2].Trim());
                    name = values[2].Trim();
                    coords = new Vector3(float.Parse(values[5].Trim()), float.Parse(values[6].Trim()),
                        float.Parse(values[7].Trim()));
                    // coords = values[0].Equals("ATOM") ? new Vector3(float.Parse(values[6].Trim()), float.Parse(values[7].Trim()),
                    //     float.Parse(values[8].Trim())) : new Vector3(float.Parse(values[5].Trim()), float.Parse(values[6].Trim()),
                    //     float.Parse(values[7].Trim()));
                // }

                
            }

        }
    }

    public class BondInfo
    {
        public int order;
        public int from;
        public int to;
        public bool breakable = false;

        //For adding bonds to existing atoms in prefabs
        public string fromName;
        public string toName;

        public BondInfo(string text, bool isPdb, List<AtomInfo> atoms)
        {
            string[] values = text.Split(new string[] {","}, StringSplitOptions.None);
            order = int.Parse(values[0]);
            from = int.Parse(values[1]);
            to = int.Parse(values[2]);

            if (isPdb)
            {
                fromName = atoms[from - 1].name;
                toName = atoms[to - 1].name;
            }
        }

        public BondInfo(int order, int from, int to)
        {
            this.order = order;
            this.from = from;
            this.to = to;
        }
    }

    public class MoleculeInfo
    {
        public string name;
        public List<AtomInfo> atoms;
        public List<BondInfo> bonds;

        public MoleculeInfo(string fileName, List<AtomInfo> atoms, List<BondInfo> bonds)
        {
            name = fileName;
            this.atoms = atoms;
            this.bonds = bonds;
        }
    }
}

