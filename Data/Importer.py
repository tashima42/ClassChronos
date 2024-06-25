import re
import os
import sqlite3
from bs4 import BeautifulSoup

# Configurable part: list of dictionaries with html_file and department_id
# DACOM = 2569
# DAMAT = 
# DAELT =
# DAMEC =
config = [
    {"html_file": "ADS.html", "department_id": "2569"},
]

# HTML tag combination to remove
tag_combination = """
<tr>
    <td class="bl">Turma</td>
    <td class="bc">Matricula Intercampus</td>
    <td class="bc">Enquadramento</td>
    <td class="br">Vagas <br> Total</td>
    <td class="br">Vagas <br> Calouros</td>
    <td class="bc">Reserva</td>
    <td class="dn">&nbsp;</td>
    <td class="bl">Prioridade - Curso</td>
    <td class="bl">Horário (dia/turno/aula)</td>
    <td class="bl">Professor (Sujeito à alteração)</td>
    <td class="bl">Optativa (Observar Equivalências)</td>
</tr>
"""

def remove_tag_combination(file_path, tag_comb):
    with open(file_path, "r", encoding="latin-1") as file:
        content = file.read()

    # Remove the tag combination, ignoring spaces and newlines
    tag_comb_pattern = re.escape(tag_comb).replace("\\ ", "\\s*").replace("\\\n", "\\s*")
    content = re.sub(tag_comb_pattern, "", content, flags=re.MULTILINE)

    # Remove additional specified tags
    additional_tags = [
        r'<td\s+align="center">\s*(.*?)\s*</td>',
        r'<td\s+class="sl"\s+tiptitle=.*?>\s*(.*?)\s*</td>',
        r'<td\s+class="sr">\s*(.*?)\s*</td>',
        r'<td\s+class="ml">\s*(?:(?:[0-9]+\s*-\s*.*?)|(?:Não)|(?:Matriz))\s*</td>',
        r'<td\s+class="sc">\s*(.*?)\s*</td>'
    ]

    for pattern in additional_tags:
        content = re.sub(pattern, "", content, flags=re.IGNORECASE | re.DOTALL)

    with open(file_path, "w", encoding="latin-1") as file:
        file.write(content)

def extract_teacher_names(file_path):
    with open(file_path, "r", encoding="latin-1") as file:
        content = file.read()

    soup = BeautifulSoup(content, 'html.parser')
    teacher_names = []
    
    # Extract teacher names
    for td in soup.find_all('td', class_='ml'):
        text = td.get_text(strip=True)
        if text and text not in ('Não', '&nbsp;') and not re.match(r'^(Arquivo|Matriz|1)', text):
            teacher_names.append(text)

    return teacher_names

def insert_teachers_to_db(teachers, department_id):
    conn = sqlite3.connect('Database.sqlite3')
    cursor = conn.cursor()
    cursor.execute('''
        CREATE TABLE IF NOT EXISTS Teacher (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT UNIQUE,
            DepartmentId TEXT
        )
    ''')

    # Check existing names to avoid duplicates
    cursor.execute('SELECT Name FROM Teacher')
    existing_teachers = {row[0] for row in cursor.fetchall()}

    # Insert unique teacher names
    for name in teachers:
        if name not in existing_teachers:
            cursor.execute('INSERT INTO Teacher (Name, DepartmentId) VALUES (?, ?)', (name, department_id))
            existing_teachers.add(name)  # Update the set with the new name to avoid duplicates in the same run

    conn.commit()
    conn.close()

def extract_class_data(file_path):
    with open(file_path, "r", encoding="latin-1") as file:
        content = file.read()

    soup = BeautifulSoup(content, 'html.parser')
    class_data = []
    current_class = {}

    for row in soup.find_all('tr'):
        tds = row.find_all('td')
        if len(tds) == 1 and 't' in tds[0].get('class', []):
            # Found a class name and number
            name_tag = tds[0].find('b')
            number_tag = tds[0].contents[-1] if len(tds[0].contents) > 0 else None
            current_class = {
                "Name": name_tag.get_text(strip=True) if name_tag else None,
                "Number": number_tag.strip() if number_tag and isinstance(number_tag, str) else None
            }
        elif len(tds) >= 4:
            # Found class details and teacher
            current_class["Code"] = tds[0].get_text(strip=True)
            current_class["Period"] = tds[2].get_text(strip=True) if len(tds) > 2 else ""
            current_class["TeacherName"] = tds[3].get_text(strip=True) if len(tds) > 3 else None
            class_data.append(current_class.copy())
    
    return class_data

def insert_classes_to_db(classes):
    conn = sqlite3.connect('Database.sqlite3')
    cursor = conn.cursor()
    cursor.execute('''
        CREATE TABLE IF NOT EXISTS Class (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT,
            Code TEXT,
            Period TEXT,
            Number INTEGER,
            TeacherId INTEGER,
            FOREIGN KEY (TeacherId) REFERENCES Teacher (Id)
        )
    ''')

    # Retrieve teacher IDs
    cursor.execute('SELECT Name, Id FROM Teacher')
    teacher_ids = {row[0]: row[1] for row in cursor.fetchall()}

    for cls in classes:
        teacher_id = teacher_ids.get(cls["TeacherName"])
        if teacher_id:
            # Calculate Number based on the number of "-"
            if cls["Period"].strip().upper() == "REMOTA" or cls['Period']=="":
                number = 0
                cls['Period'] = "REMOTA"
            else:
                number = cls["Period"].count("-") + 1
            
            cursor.execute('''
                INSERT INTO Class (Name, Code, Period, Number, TeacherId) 
                VALUES (?, ?, ?, ?, ?)
            ''', (cls["Name"], cls["Code"], cls["Period"], number, teacher_id))

    conn.commit()
    conn.close()

# Process each file in the config
for entry in config:
    html_file = entry["html_file"]
    department_id = entry["department_id"]
    
    if os.path.isfile(html_file):
        remove_tag_combination(html_file, tag_combination)
        print(f"Updated {html_file} for department {department_id} by removing the specified tag combination and additional tags.")
        
        teacher_names = extract_teacher_names(html_file)
        insert_teachers_to_db(teacher_names, department_id)
        print(f"Inserted teachers from {html_file} into the database.")
        
        class_data = extract_class_data(html_file)
        insert_classes_to_db(class_data)
        print(f"Inserted classes from {html_file} into the database.")
    else:
        print(f"File {html_file} not found.")
