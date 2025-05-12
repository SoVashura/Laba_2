# Лексический анализатор для структур языка C

## Персональный вариант задания:

**Анализ объявления и определения структуры на языке C:**
```c
struct str1 {
    int i;
    char c;
    float f;
};
```

## Примеры допустимых строк:

1. Обычная структура с полями:
```c
struct str1 {
    int i;
    char c;
    float f;
};
```
2. Пустая структура (без полей):
```c
struct str1 {
};
```

## Разработанная грамматика:

Определим грамматику объявления и определения структуры на языке С G[‹Start›] в нотации Хомского с продукциями P:
```bnf
1.  ‹Start›   → 'struct' ‹Struct›
2.  ‹Struct›  → ' ' ‹Name›
3.  ‹Name›    → ‹Letter› ‹NameRem›
4.  ‹NameRem› → ‹Letter› ‹NameRem› 
              | ‹Digit› ‹NameRem› 
              | '{' ‹X›
5.  ‹X›       → 'type' ‹Space›
6.  ‹Space›   → ' ' ‹Y›
7.  ‹Y›       → ‹Letter› ‹YRem›
8.  ‹YRem›    → ‹Letter› ‹YRem› 
              | ‹Digit› ‹YRem› 
              | ';' ‹X›
9.  ‹X›       → '}' ‹End›
10. ‹End›     → ';'
