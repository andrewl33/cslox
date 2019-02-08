#!/usr/bin/python3

# Generates Expr.cs file

base_name = "Test"


def define_type(f, base_name, class_name, fields):

    # Definition.
    f.write(f"\tpublic class {class_name} : {base_name}\n")
    f.write("\t{\n")

    # Fields.
    for expr in [e.strip() for e in fields.split(",")]:
        f.write(f"\t\tpublic readonly {expr};\n")

    # Constructor.
    f.write(f"\t\tpublic {class_name}({fields})\n")
    f.write("\t\t{\n")

    for expr in [e.split()[1].strip() for e in fields.split(",")]:
        f.write(f"\t\t\tthis.{expr} = {expr};\n")

    f.write("\t\t}\n")

    f.write("\t}\n\n")


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
        f.write("\t{\n\t}\n")

        # Write classes.
        for expression in expressions:
            class_name = expression.split(":")[0].strip()
            fields = expression.split(":")[1].strip()
            define_type(f, base_name, class_name, fields)

        # Write footer.
        f.write("}\n")


def main():
    define_ast("Expr", ["Binary   : Expr left, Token op, Expr right",
                        "Grouping : Expr expression",
                        "Literal  : Object value",
                        "Unary    : Token op, Expr right"])


main()
