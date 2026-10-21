grammar LabCalculator;

compileUnit: expression EOF;

expression:
      LPAREN expression RPAREN                             #ParenthesizedExpr
    | NOT expression                                       #NotExpr
    | MMAX LPAREN paramlist RPAREN                         #MmaxExpr
    | MMIN LPAREN paramlist RPAREN                         #MminExpr
    | expression operatorToken=(MULTIPLY | DIVIDE | MOD | DIV) expression #MultiplicativeExpr
    | expression operatorToken=(ADD | SUBTRACT) expression #AdditiveExpr
    | expression operatorToken=(LTEQ | GTEQ | NEQ) expression #ComparisonExpr
    | NUMBER                                               #NumberExpr
    | IDENTIFIER                                           #IdentifierExpr
    ;

paramlist: expression (',' expression)*;

NUMBER : [0-9]+;
IDENTIFIER : [a-zA-Z]+[0-9]+; // для клітинок типу A1, B2

ADD : '+';
SUBTRACT : '-';
MULTIPLY : '*';
DIVIDE : '/';
MOD : 'mod';
DIV : 'div';

LTEQ : '<=';
GTEQ : '>=';
NEQ  : '<>';

NOT : 'not';
MMAX : 'mmax';
MMIN : 'mmin';

LPAREN : '(';
RPAREN : ')';

WS : [ \t\r\n]+ -> skip;