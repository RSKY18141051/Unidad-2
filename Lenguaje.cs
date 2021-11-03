using System;
using System.Collections.Generic;
using System.Text;

/*
Requerimiento 1: Implementar el NOT en el IF.
Requerimiento 2: Validar la asignacion de STRINGS en Instruccion.
Requerimiento 3: Implementar la comparacion de tipos de datos en Lista_IDs.
Requerimiento 4: Validar los tipos de datos en asignacion de cin.
Requerimiento 5: Implementar el cast.
Completados: R1(11/10), R2(11/10), R3(12/10), R4(12/10), R5(14/10)
*/

namespace sintaxis3
{
    class Lenguaje : Sintaxis
    {
        Stack s;
        ListaVariables l;
        Variable.tipo maxBytes;
        public Lenguaje()
        {
            s = new Stack(5);
            l = new ListaVariables();
            Console.WriteLine("Iniciando analisis gramatical.");
        }

        public Lenguaje(string nombre) : base(nombre)
        {
            s = new Stack(5);
            l = new ListaVariables();
            Console.WriteLine("Iniciando analisis gramatical.");
        }
        //Programa -> Libreria Main
        public void Programa()
        {
            Libreria();
            Main();
            l.imprime(bitacora);
        }

        //Libreria -> (#include <identificador(.h)?> Libreria) ?
        private void Libreria()
        {
            if (getContenido() == "#")
            {
                match("#");
                match("include");
                match("<");
                match(clasificaciones.identificador);
                if (getContenido() == ".")
                {
                    match(".");
                    match("h");
                }
                match(">");
                Libreria();
            }
        }
        //Main -> tipoDato main() BloqueInstrucciones 
        private void Main()
        {
            match(clasificaciones.tipoDato);
            match("main");
            match("(");
            match(")");
            BloqueInstrucciones(true);
        }
        //BloqueInstrucciones -> { Instrucciones }
        private void BloqueInstrucciones(bool ejecuta)
        {
            match(clasificaciones.inicioBloque);
            Instrucciones(ejecuta);
            match(clasificaciones.finBloque);
        }

        //Lista_IDs -> identificador (= Expresion)? (,Lista_IDs)? 
        private void Lista_IDs(Variable.tipo Tipos, bool ejecuta)
        {
            string nombre = getContenido();
            match(clasificaciones.identificador);
            if (!l.Existe(nombre))
            {
                l.Inserta(nombre, Tipos);
            }
            else
            {
                throw new Error(bitacora, "Error de sintaxis: Variable duplicada (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
            }

            if (getClasificacion() == clasificaciones.asignacion)
            {
                match(clasificaciones.asignacion);
                string contenidos = getContenido();
                if (getClasificacion() == clasificaciones.cadena)
                {
                    if (Tipos == Variable.tipo.STRING)
                    {
                        match(clasificaciones.cadena);
                        if (ejecuta)
                        {
                            l.setValor(nombre, contenidos);
                        }
                    }
                    else
                    {
                        throw new Error(bitacora, "Error de semantico: No se puede asignar un STRING a un (" + Tipos + ") " + "(" + linea + ", " + caracter + ")");
                    }
                }
                else
                {
                    //Requerimiento 3
                    string valor;
                    maxBytes = Variable.tipo.CHAR;
                    Expresion();
                    valor = s.pop(bitacora, linea, caracter).ToString();
                    if (tipoDatoExpresion(float.Parse(valor)) > maxBytes)
                    {
                        maxBytes = tipoDatoExpresion(float.Parse(valor));
                    }
                    if (l.getTipoDato(nombre) < maxBytes)
                    {
                        throw new Error(bitacora, "Error de semantico: No se puede asignar un " + maxBytes + " a un (" + l.getTipoDato(nombre) + ") " + "(" + linea + ", " + caracter + ")");
                    }
                    if (ejecuta)
                    {
                        l.setValor(nombre, s.pop(bitacora, linea, caracter).ToString());
                    }
                }
            }

            if (getContenido() == ",")
            {
                match(",");
                Lista_IDs(Tipos, ejecuta);
            }
        }

        //Variables -> tipoDato Lista_IDs; 
        private void Variables(bool ejecuta)
        {
            string contenidos = getContenido();
            match(clasificaciones.tipoDato);
            Variable.tipo Tipos;
            switch (contenidos)
            {
                case "const":
                    Tipos = Variable.tipo.CONST;
                    break;
                case "int":
                    Tipos = Variable.tipo.INT;
                    break;
                case "float":
                    Tipos = Variable.tipo.FLOAT;
                    break;
                case "char":
                    Tipos = Variable.tipo.CHAR;
                    break;
                case "string":
                    Tipos = Variable.tipo.STRING;
                    break;
                default:
                    Tipos = Variable.tipo.CHAR;
                    break;
            }
            Lista_IDs(Tipos, ejecuta);

            match(clasificaciones.finSentencia);
        }
        //Instruccion -> (If | cin | cout | const | Variables | asignacion) ;
        private void Instruccion(bool ejecuta)
        {
            if (getContenido() == "do")
            {
                DOWHILE(ejecuta);
            }
            else if (getContenido() == "while")
            {
                WHILE(ejecuta);
            }
            else if (getContenido() == "for")
            {
                FOR(ejecuta);
            }
            else if (getContenido() == "if")
            {
                IF(ejecuta);
            }
            else if (getContenido() == "cin")
            {
                //Requerimiento 4
                match("cin");
                match(clasificaciones.flujoEntrada);
                string nombre = getContenido();
                string valor = Console.ReadLine();
                if (tipoDatoExpresion(float.Parse(valor)) > maxBytes)
                {
                    maxBytes = tipoDatoExpresion(float.Parse(valor));
                }
                if (l.getTipoDato(nombre) < maxBytes)
                {
                    throw new Error(bitacora, "Error de semantico: No se puede asignar un " + maxBytes + " a un (" + l.getTipoDato(nombre) + ") " + "(" + linea + ", " + caracter + ")");
                }

                string contenido = getContenido();
                if (l.Existe(contenido))
                {
                    match(clasificaciones.identificador);
                    if (ejecuta)
                    {
                        string lector = Console.ReadLine();
                        l.setValor(contenido, lector);
                    }
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: Variable no existe (" + contenido + ") " + "(" + linea + ", " + caracter + ")");
                }
                match(clasificaciones.finSentencia);
            }
            else if (getContenido() == "cout")
            {
                match("cout");
                ListaFlujoSalida(ejecuta);
                match(clasificaciones.finSentencia);
            }
            else if (getContenido() == "const")
            {
                Constante(ejecuta);
            }
            else if (getClasificacion() == clasificaciones.tipoDato)
            {
                Variables(ejecuta);
            }
            else
            {
                string nombre = getContenido();
                if (l.Existe(nombre))
                {
                    match(clasificaciones.identificador);
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: Variable no existe (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
                }
                match(clasificaciones.asignacion);

                string valor;
                //Requerimiento 2
                if (getClasificacion() == clasificaciones.cadena)
                {
                    valor = getContenido();
                    //match(clasificaciones.cadena);
                    if (l.getTipoDato(nombre) == Variable.tipo.STRING)
                    {
                        match(clasificaciones.cadena);
                        if (ejecuta)
                        {
                            l.setValor(nombre, valor);
                        }
                    }
                    else
                    {
                        throw new Error(bitacora, "Error de semantico: No se puede asignar un STRING a un (" + l.getTipoDato(nombre) + ") " + "(" + linea + ", " + caracter + ")");
                    }
                }
                else
                {
                    //Requerimiento 3
                    maxBytes = Variable.tipo.CHAR;
                    Expresion();
                    valor = s.pop(bitacora, linea, caracter).ToString();
                    if (tipoDatoExpresion(float.Parse(valor)) > maxBytes)
                    {
                        maxBytes = tipoDatoExpresion(float.Parse(valor));
                    }
                    if (l.getTipoDato(nombre) < maxBytes)
                    {
                        throw new Error(bitacora, "Error de semantico: No se puede asignar un " + maxBytes + " a un (" + l.getTipoDato(nombre) + ") " + "(" + linea + ", " + caracter + ")");
                    }
                }
                if (ejecuta)
                {
                    l.setValor(nombre, valor);
                }
                match(clasificaciones.finSentencia);
            }
        }
        //Instrucciones -> Instruccion Instrucciones?
        private void Instrucciones(bool ejecuta)
        {
            Instruccion(ejecuta);
            if (getClasificacion() != clasificaciones.finBloque)
            {
                Instrucciones(ejecuta);
            }
        }

        //Constante -> const tipoDato identificador = numero | cadena;
        private void Constante(bool ejecuta)
        {
            match("const");
            string contenidos = getContenido();
            match(clasificaciones.tipoDato);
            Variable.tipo Tipos;
            switch (contenidos)
            {
                case "const":
                    Tipos = Variable.tipo.CONST;
                    break;
                case "int":
                    Tipos = Variable.tipo.INT;
                    break;
                case "float":
                    Tipos = Variable.tipo.FLOAT;
                    break;
                case "char":
                    Tipos = Variable.tipo.CHAR;
                    break;
                case "string":
                    Tipos = Variable.tipo.STRING;
                    break;
                default:
                    Tipos = Variable.tipo.CHAR;
                    break;
            }

            string content = getContenido();
            if (!l.Existe(content) && ejecuta)
            {
                match(clasificaciones.identificador);
            }
            else
            {
                throw new Error(bitacora, "Error de sintaxis: Variable no existe (" + content + ") " + "(" + linea + ", " + caracter + ")");
            }
            match(clasificaciones.asignacion);

            string normal;
            if (getClasificacion() == clasificaciones.numero)
            {
                normal = getContenido();
                match(clasificaciones.numero);
                l.setValor(content, normal);
            }
            else
            {
                normal = getContenido();
                match(clasificaciones.cadena);
                l.setValor(content, normal);
            }

            if (ejecuta)
            {
                l.setValor(content, normal);
            }
            match(clasificaciones.finSentencia);
        }
        //ListaFlujoSalida -> << cadena | identificador | numero (ListaFlujoSalida)?
        private void ListaFlujoSalida(bool ejecuta)
        {
            match(clasificaciones.flujoSalida);

            if (getClasificacion() == clasificaciones.numero)
            {
                if (ejecuta)
                {
                    Console.Write(getContenido());
                }
                match(clasificaciones.numero);
            }
            else if (getClasificacion() == clasificaciones.cadena)
            {
                string saltos = getContenido(); //Secuencias de escape
                if (saltos.Contains("\\n"))
                {
                    saltos = saltos.Replace("\\n", "\n");
                }

                if (saltos.Contains("\\t"))
                {
                    saltos = saltos.Replace("\\t", "\t");
                }

                if (saltos.Contains("\""))
                {
                    saltos = saltos.Replace("\"", "");
                }

                if (ejecuta)
                {
                    Console.Write(saltos);
                }
                match(clasificaciones.cadena);
            }
            else
            {
                string nombre = getContenido();
                if (l.Existe(nombre))
                {
                    if (ejecuta)
                    {
                        Console.Write(l.getValor(nombre));
                    }
                    match(clasificaciones.identificador);
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: Variable no existe (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
                }
            }

            if (getClasificacion() == clasificaciones.flujoSalida)
            {
                ListaFlujoSalida(ejecuta);
            }
        }
        //IF -> if (Condicion) { BloqueInstrucciones } (else BloqueInstrucciones)?
        private void IF(bool ejecuta2)
        {
            match("if");
            match("(");
            bool ejecuta = Condicion();
            match(")");
            BloqueInstrucciones(ejecuta && ejecuta2);

            if (getContenido() == "else")
            {
                match("else");
                BloqueInstrucciones(!ejecuta && ejecuta2);
            }
        }
        //Condicion -> Expresion operadorRelacional Expresion
        private bool Condicion()
        {
            //Requerimiento 1
            bool negar = false; 
            if (getContenido() == "!")
            {
                match("!");
                match("(");
                negar = true;
            }
            maxBytes = Variable.tipo.CHAR;
            Expresion();
            float n1 = s.pop(bitacora, linea, caracter);
            string operador = getContenido();
            match(clasificaciones.operadorRelacional);
            maxBytes = Variable.tipo.CHAR;
            Expresion();
            float n2 = s.pop(bitacora, linea, caracter);

            if (negar == true)
            {
                match(")");
            }
            switch (operador)
            {
                case ">":
                    return (negar ? !(n1 > n2) : n1 > n2);
                case ">=":
                    return (negar ? !(n1 >= n2) : n1 >= n2);
                case "<":
                    return (negar ? !(n1 < n2) : n1 < n2);
                case "<=":
                    return (negar ? !(n1 <= n2) : n1 <= n2);
                case "==":
                    return (negar ? !(n1 == n2) : n1 == n2);
                default:
                    /*case "!=":
                    case "<>":*/
                    return (negar ? !(n1 != n2) : n1 != n2);
            }
        }
        //Expresion -> Termino MasTermino 
        private void Expresion()
        {
            Termino();
            MasTermino();
        }
        //MasTermino -> (operadorTermino Termino)?
        private void MasTermino()
        {
            if (getClasificacion() == clasificaciones.operadorTermino)
            {
                string operador = getContenido();
                match(clasificaciones.operadorTermino);
                Termino();
                float e1 = s.pop(bitacora, linea, caracter), e2 = s.pop(bitacora, linea, caracter);
                switch (operador)
                {
                    case "+":
                        s.push(e2 + e1, bitacora, linea, caracter);
                        break;
                    case "-":
                        s.push(e2 - e1, bitacora, linea, caracter);
                        break;
                }
                s.display(bitacora);
            }
        }
        //Termino -> Factor PorFactor
        private void Termino()
        {
            Factor();
            PorFactor();
        }
        //PorFactor -> (operadorFactor Factor)?
        private void PorFactor()
        {
            if (getClasificacion() == clasificaciones.operadorFactor)
            {
                string operador = getContenido();
                match(clasificaciones.operadorFactor);
                Factor();
                float e1 = s.pop(bitacora, linea, caracter), e2 = s.pop(bitacora, linea, caracter);
                switch (operador)
                {
                    case "*":
                        s.push(e2 * e1, bitacora, linea, caracter);
                        break;
                    case "/":
                        s.push(e2 / e1, bitacora, linea, caracter);
                        break;
                }
                s.display(bitacora);
            }
        }
        //Factor -> identificador | numero | ( Expresion )
        private void Factor()
        {
            if (getClasificacion() == clasificaciones.identificador)
            {
                string nombre = getContenido();
                if (l.Existe(nombre))
                {
                    s.push(float.Parse(l.getValor(getContenido())), bitacora, linea, caracter);
                    s.display(bitacora);
                    match(clasificaciones.identificador);
                    if (l.getTipoDato(nombre) > maxBytes)
                    {
                        maxBytes = l.getTipoDato(nombre);
                    }
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: Variable no existe (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
                }
            }
            else if (getClasificacion() == clasificaciones.numero)
            {
                //Console.Write(getContenido() + " ");
                s.push(float.Parse(getContenido()), bitacora, linea, caracter);
                s.display(bitacora);
                if (tipoDatoExpresion(float.Parse(getContenido())) > maxBytes)
                {
                    maxBytes = tipoDatoExpresion(float.Parse(getContenido()));
                }
                match(clasificaciones.numero);
            }
            else
            {
                match("(");
                bool huboCast = false;
                Variable.tipo tipoDato = Variable.tipo.CHAR;
                if (getClasificacion() == clasificaciones.tipoDato)
                {
                    huboCast = true;
                    tipoDato = determinarTipoDato(getContenido());
                    match(clasificaciones.tipoDato);
                    match(")");
                    match("(");
                }
                Expresion();
                match(")");
                if (huboCast)
                {
                    //Hacer un pop y convertir ese numero a el tipoDato y meterlo al stack.
                    float n1 = s.pop(bitacora, linea, caracter);
                    /*
                    -Para convertir un int a char necesitamos dividir ente 256 y el residuo es el resultado del cast
                    dominio de char 0 a 255, si se suma 1 es 0, 256 = 0, 257 = 1, 258 = 2, ....
                    -Para convertir un float a int necesitamos dividir ente 65536 y el residuo es el resultado del cast
                    -Para convertir un float a otro tipo de dato redondear el numero para eliminar la parte fraccional
                    -Para convertir un float a char necesitamos dividir ente 65536/256 y el residuo es el resultado del cast
                    -Para convertir a float n1 = n1
                    */
                    n1 = CAST(n1, tipoDato);
                    s.push(n1, bitacora, linea, caracter);
                    maxBytes = tipoDato;
                }
            }
        }
        //FOR -> for (identificador = Expresion; Condicion; identificador incrementoTermino) BloqueInstrucciones
        private void FOR(bool ejecuta)
        {
            match("for");
            match("(");
            string nombre = getContenido();
            if (l.Existe(nombre))
            {
                match(clasificaciones.identificador);
            }
            else
            {
                throw new Error(bitacora, "Error de sintaxis: Variable no existe (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
            }
            match(clasificaciones.asignacion);
            Expresion();
            match(clasificaciones.finSentencia);
            Condicion();
            match(clasificaciones.finSentencia);
            string contenido = getContenido(); //Variables diferentes para evitar confusion
            if (l.Existe(contenido))
            {
                match(clasificaciones.identificador);
            }
            else
            {
                throw new Error(bitacora, "Error de sintaxis: Variable no existe (" + contenido + ") " + "(" + linea + ", " + caracter + ")");
            }
            match(clasificaciones.incrementoTermino);
            match(")");
            BloqueInstrucciones(ejecuta);
        }
        //WHILE -> while (Condicion) BloqueInstrucciones
        private void WHILE(bool ejecuta)
        {
            match("while");
            match("(");
            Condicion();
            match(")");
            BloqueInstrucciones(ejecuta);
        }
        //DOWHILE -> do BloqueInstrucciones while (Condicion);
        private void DOWHILE(bool ejecuta)
        {
            match("do");
            BloqueInstrucciones(ejecuta);
            match("while");
            match("(");
            Condicion();
            match(")");
            match(clasificaciones.finSentencia);
        }
        private Variable.tipo determinarTipoDato(string tipoDato)
        {
            Variable.tipo tipoVar;
            switch (tipoDato)
            {
                case "int":
                    tipoVar = Variable.tipo.INT;
                    break;

                case "float":
                    tipoVar = Variable.tipo.FLOAT;
                    break;

                case "string":
                    tipoVar = Variable.tipo.STRING;
                    break;

                default:
                    tipoVar = Variable.tipo.CHAR;
                    break;
            }
            return tipoVar;
        }
        private Variable.tipo tipoDatoExpresion(float valor)
        {
            if (valor % 1 != 0)
            {
                return Variable.tipo.FLOAT;
            }
            else if (valor < 256)
            {
                return Variable.tipo.CHAR;
            }
            else if (valor < 65535)
            {
                return Variable.tipo.INT;
            }
            return Variable.tipo.FLOAT;
        }  
        private float CAST(float n1, Variable.tipo casteo)
        {
            switch (casteo)
            {
                case Variable.tipo.INT:
                    if (tipoDatoExpresion(n1) == Variable.tipo.FLOAT)
                    {
                        n1 = n1 % 65536;
                    }
                    break;
                case Variable.tipo.CHAR:
                    if (tipoDatoExpresion(n1) == Variable.tipo.FLOAT)
                    {
                        n1 = n1 % (65536 / 256);
                    }
                    else if (tipoDatoExpresion(n1) == Variable.tipo.INT)
                    {
                        n1 = n1 % 256;
                    }
                    break;
                case Variable.tipo.FLOAT:
                    break;
                default:
                    if (tipoDatoExpresion(n1) == Variable.tipo.CHAR || tipoDatoExpresion(n1) == Variable.tipo.INT || tipoDatoExpresion(n1) == Variable.tipo.FLOAT)
                    {
                        n1 = n1;
                    }
                    break;
            }
            return n1;
        }
    }
}