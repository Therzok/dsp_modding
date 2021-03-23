meta:
  id: dyson_ui
  title: SaveTheWindows storage format
  license: MIT
  endian: le
doc: |
  This storage format is used to save/restore window positions of RectTransform for use on https://ide.kaitai.io/
seq:
  - id: version
    type: s4
    
  - id: source
    type: csharp_string
  
  - id: windows
    type: windows_data

types:
  windows_data:
    seq:
      - id: len
        type: s4
    
      - id: window_data
        type: window_data
        repeat: expr
        repeat-expr: len
    
  window_data:
    seq:
      - id: name
        type: csharp_string
      - id: x
        type:
          switch-on: _root.version
          cases:
            1: f4
            2: u4
      - id: y
        type:
          switch-on: _root.version
          cases:
            1: f4
            2: u4
        
  csharp_string:
    seq:
      - id: len
        type: u1
      - id: value
        type: str
        encoding: UTF-8
        size: len

