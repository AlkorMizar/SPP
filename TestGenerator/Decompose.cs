using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestGenerator
{
    
    class Decompose
    {
        string GetTypeName(TypeDeclarationSyntax type) {
            return type.Identifier.ValueText;
        }

        string GetNamespace(NamespaceDeclarationSyntax namesp) {
            return namesp.Name.ToString();
        }

        private string[] GetMethodsNames(TypeDeclarationSyntax type)
        {
            var publicMethodsNames = from methodDeclaration in type.DescendantNodes()
                                                                                 .OfType<MethodDeclarationSyntax>()
                                     where methodDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword)
                                     select methodDeclaration.Identifier.ToString();

            return publicMethodsNames.Cast<string>().ToArray();
        }
        public IEnumerable<TypeContext> DecomposeType(string programText) { 
            var list = new List<TypeContext>();
            SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            foreach (var member in root.Members)
            {
                var namespaceDecl = member as NamespaceDeclarationSyntax;
                if (namespaceDecl!=null) {
                    foreach (var memberType in namespaceDecl.Members)
                    {
                        if (memberType.Kind()==SyntaxKind.ClassDeclaration || memberType.Kind() == SyntaxKind.StructDeclaration) {
                            var type = memberType as TypeDeclarationSyntax;
                            
                            list.Add(new TypeContext(GetNamespace(namespaceDecl), GetTypeName(type), GetMethodsNames(type)));
                        }
                    }
                }
            }
            return list;
        }
    }
}
