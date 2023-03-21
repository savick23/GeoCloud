//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание:
//--------------------------------------------------------------------------------------------------
using RmSolution.Utils;

bool compress = true;
for (int i = 0; i < args.Length; i += 2)
{
    if (args[0] == "-no")
    {
        compress = false;
        i--;
        continue;
    }

    switch (Path.GetExtension(args[i + 1]))
    {
        case ".smx":
            InitCompressor.Concat(args[i], args[i + 1], i + 2 < args.Length ? args[i++ + 2] : "-release");
            break;
    }
}