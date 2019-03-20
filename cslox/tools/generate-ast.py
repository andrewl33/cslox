#!/usr/bin/python3

# Generates Expr.cs file

base_name = "Test"

def define_visitor(f, base_name, types):
    # write interface
    f.write("\t\tpublic interface IVisitor<R>\n\t\t{\n")

    for s in types:
        type_name = s.split(":")[0].strip()
        f.write(f"\t\t\tR Visit{type_name}{base_name}({type_name} {base_name.lower()});\n")

    f.write("\t\t}\n")

def define_type(f, base_name, class_name, fields):

    # Definition.
    f.write(f"\t\tpublic class {class_name} : {base_name}\n")
    f.write("\t\t{\n")

    # Fields.
    for expr in [e.strip() for e in fields.split(",")]:
        f.write(f"\t\t\tpublic readonly {expr};\n")

    # Constructor.
    f.write(f"\t\t\tpublic {class_name}({fields})\n")
    f.write("\t\t\t{\n")

    for expr in [e.split()[1].strip() for e in fields.split(",")]:
        f.write(f"\t\t\t\tthis.{expr} = {expr};\n")

    f.write("\t\t\t}\n")
    
    # Visitor Pattern.
    f.write("\t\t\tpublic override R Accept<R>(IVisitor<R> visitor)\n")
    f.write("\t\t\t{\n")
    f.write(f"\t\t\t\treturn visitor.Visit{class_name}{base_name}(this);\n")
    f.write("\t\t\t}\n")
    f.write("\t\t}\n\n")


def define_ast(base_name, expressions):
    with open(f"../{base_name}.cs", "w") as f:

        # Write header.
        f.write("using System;\n")
        f.write("using System.Collections.Generic;\n")
        f.write("using System.Text;\n")
        f.write("\n")

        # Write namespace.
        f.write("namespace cslox\n{\n")



        # Write abstract base class for all Expr.
        f.write(f"\tpublic abstract class {base_name}\n")
        f.write("\t{\n")
        f.write("\t\tpublic abstract R Accept<R>(IVisitor<R> visitor);\n")
 
        
        # Write Visitor Interface.
        define_visitor(f, base_name, expressions)

        # Write classes.
        for expression in expressions:
            class_name = expression.split(":")[0].strip()
            fields = expression.split(":")[1].strip()
            define_type(f, base_name, class_name, fields)

        # Write end of abstract vase class.
        f.write("\t}\n")

        # Write footer.
        f.write("}\n")


def main():
    define_ast("Expr", ["Assign   : Token name, Expr value",
                        "Binary   : Expr left, Token op, Expr right",
                        "Grouping : Expr expression",
                        "Literal  : object value",
                        "Unary    : Token op, Expr right",
                        "Variable : Token name"])

    define_ast("Stmt", ["Block      : List<Stmt> statements",
                        "Expression : Expr expression",
                        "Print      : Expr expression",
                        "Var        : Token name, Expr initializer"])


main()
